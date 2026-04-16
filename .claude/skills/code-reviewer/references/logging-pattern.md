# Logging Pattern Guide

Standard logging patterns for the .NET 9 microservices solution.

## Setup & Configuration

### Program.cs - Serilog Configuration

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with structured logging
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Debug()  // or read from config: .MinimumLevel.Override(...)
        .WriteTo.Console(outputTemplate: 
            "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", typeof(Program).Assembly.GetName().Name)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
);

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

app.Run();
```

### DI Registration

Always inject `ILogger<T>` where T is the class using it:

```csharp
public class ProductService
{
    private readonly ILogger<ProductService> _logger;

    public ProductService(ILogger<ProductService> logger)
    {
        _logger = logger;
    }
}
```

## Logging Levels

| Level | When to Use | Example |
|-------|-----------|---------|
| **Debug** | Detailed flow, intermediate values | `"Deserialized entity: {@Product}..."` |
| **Information** | Business events, successful operations | `"Product created with ID: {Id}"` |
| **Warning** | Unexpected but recoverable situations | `"Product not found: {Id}"` |
| **Error** | Exceptions, operation failures | `"Failed to save product: {Id}"` exception |
| **Fatal** | Application-stopping errors | `"Database connection permanently lost"` |

**Do NOT log at Information level for every line.** Use Debug for verbose flow, Information for important business events.

## Structured Logging Patterns

Always use **named parameters** (not string concatenation) for structured logging:

```csharp
// GOOD - Structured parameters for searchability
_logger.LogInformation("Processing order {OrderId} for customer {CustomerId}", 
    orderId, customerId);

// BAD - String concatenation (loses context)
_logger.LogInformation($"Processing order {orderId} for customer {customerId}");

// GOOD - Complex objects as properties
_logger.LogDebug("Basket details: {@Basket}", basket);

// Query later: Find all logs where OrderId = "123"
```

## Common Logging Patterns

### Public Method Entry/Exit

```csharp
public async Task<ProductDto> GetProductAsync(int productId)
{
    _logger.LogInformation("Getting product: {ProductId}", productId);
    try
    {
        var product = await _repository.GetProductByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            return null;
        }
        _logger.LogDebug("Retrieved product: {ProductName}", product.Name);
        return product.MapToDto();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving product: {ProductId}", productId);
        throw;
    }
}
```

### Data Access (Repository)

```csharp
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public async Task<Product> GetProductByIdAsync(int id)
    {
        _logger.LogDebug("Fetching product from database: {ProductId}", id);
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                _logger.LogDebug("Product not found in database: {ProductId}", id);
            else
                _logger.LogDebug("Successfully retrieved product: {@Product}", 
                    new { product.Id, product.Name });
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error fetching product: {ProductId}", id);
            throw;
        }
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        _logger.LogInformation("Creating new product: {ProductName}", product.Name);
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Product created successfully with ID: {ProductId}", 
                product.Id);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", product.Name);
            throw;
        }
    }
}
```

### Validation & Failed Operations

```csharp
public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
{
    _logger.LogInformation("Creating product: {ProductName}", request.Name);
    
    // Validation
    var validationResult = await _validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
        _logger.LogWarning("Product creation validation failed: {Errors}", errors);
        throw new ValidationException(validationResult.Errors);
    }

    try
    {
        var product = new Product(request.Name, request.Price);
        var created = await _repository.CreateProductAsync(product);
        _logger.LogInformation("Product created with ID: {ProductId}", created.Id);
        return created.MapToDto();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error creating product: {ProductName}", request.Name);
        throw;
    }
}
```

### Async Operations

```csharp
public async Task<List<OrderDto>> GetOrdersAsync(string customerId)
{
    _logger.LogInformation("Fetching orders for customer: {CustomerId}", customerId);
    
    try
    {
        _logger.LogDebug("Querying database for customer orders: {CustomerId}", customerId);
        var orders = await _repository.GetOrdersByCustomerAsync(customerId);
        
        _logger.LogDebug("Retrieved {OrderCount} orders for customer: {CustomerId}", 
            orders.Count, customerId);
        
        var dtos = orders.Select(o => o.MapToDto()).ToList();
        _logger.LogInformation("Successfully retrieved orders: {OrderCount}", dtos.Count);
        
        return dtos;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching orders for customer: {CustomerId}", customerId);
        throw;
    }
}
```

### Cache Operations

```csharp
public async Task<ProductDto> GetProductFromCacheAsync(int id)
{
    var cacheKey = $"product_{id}";
    
    _logger.LogDebug("Checking cache for product: {CacheKey}", cacheKey);
    var cached = await _cache.GetAsync<ProductDto>(cacheKey);
    
    if (cached != null)
    {
        _logger.LogDebug("Cache hit for product: {ProductId}", id);
        return cached;
    }
    
    _logger.LogDebug("Cache miss for product: {ProductId}, fetching from database", id);
    var product = await _repository.GetProductByIdAsync(id);
    if (product != null)
    {
        var dto = product.MapToDto();
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30));
        _logger.LogDebug("Cached product for 30 minutes: {ProductId}", id);
        return dto;
    }
    
    _logger.LogWarning("Product not found: {ProductId}", id);
    return null;
}
```

## What NOT to Log

### Don't Log Sensitive Data

```csharp
// BAD - Logs password
_logger.LogInformation("User login: {User} with password: {Password}", user, password);

// GOOD - Logs user, not password
_logger.LogInformation("User login attempt: {User}", user);

// BAD - Logs credit card
_logger.LogInformation("Payment processed: {CardNumber}", cardNumber);

// GOOD - Logs masked info
_logger.LogInformation("Payment processed: {CardLast4}", cardNumber.Last(4).ToString().PadLeft(4, '*'));
```

### Don't Log Every Debug Line

```csharp
// BAD - Too verbose
var x = 5;
_logger.LogDebug("x = 5");
var y = x * 2;
_logger.LogDebug("y = 10");

// GOOD - Only important intermediate values
public async Task<Order> CalculateOrderTotalAsync(int orderId)
{
    _logger.LogInformation("Calculating total for order: {OrderId}", orderId);
    var items = await _repository.GetOrderItemsAsync(orderId);
    _logger.LogDebug("Retrieved {ItemCount} items for order", items.Count);
    
    var subtotal = items.Sum(i => i.Price * i.Quantity);
    var tax = subtotal * 0.10m;
    var total = subtotal + tax;
    
    _logger.LogInformation("Order total calculated: {Subtotal:C2} + {Tax:C2} = {Total:C2}", 
        subtotal, tax, total);
    return order;
}
```

### Don't Duplicate Information

```csharp
// BAD - Redundant
_logger.LogWarning("Failed to update product {ProductId}");
_logger.LogWarning("Product update failed");
_logger.LogWarning("Could not update {ProductId}");

// GOOD - One clear message
_logger.LogWarning("Failed to update product: {ProductId}", productId);
```

## Performance Considerations

- Logging is I/O bound; don't log on every iteration
- Use LogLevel guards for expensive operations:

```csharp
// GOOD - Guard for Debug (disabled in Production)
if (_logger.IsEnabled(LogLevel.Debug))
{
    _logger.LogDebug("Expensive object serialization: {@Product}", product);
}

// Or using lambda (evaluates only if level enabled)
_logger.LogDebug("Data: {Data}", () => ExpensiveSerialize(product));
```

## Summary

- **Setup**: Use Serilog with structured logging
- **Levels**: Debug for flow, Information for business events, Warning for issues, Error for exceptions
- **Patterns**: Entry/exit, try-catch logging, validation failures, async operations
- **Best Practices**: Named parameters, no sensitive data, no over-logging, guard expensive operations
