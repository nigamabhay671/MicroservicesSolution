# Code Quality Checklist

Review code quality, naming conventions, readability, and maintainability.

## Naming Conventions

- [ ] Public types (classes, interfaces): `PascalCase`
- [ ] Public methods/properties: `PascalCase`
- [ ] Private fields: `_camelCase` with underscore prefix
- [ ] Local variables/parameters: `camelCase`
- [ ] Constants: `PascalCase` or `SCREAMING_SNAKE_CASE` (if uppercase appropriate)
- [ ] Boolean properties start with "Is", "Has", "Can": `IsActive`, `HasItems`, `CanCreate`
- [ ] Async methods end with "Async": `GetProductAsync`, not `GetProduct`
- [ ] Interfaces start with "I": `IProductRepository`
- [ ] Method names are verbs or verb phrases: `GetProduct`, `CreateOrder`, `DeleteBasket`
- [ ] Parameter names are nouns: `id`, `name`, `request`, not `param1`, `data`

**Example - GOOD**:
```csharp
public interface IProductRepository
{
    Task<Product> GetProductByIdAsync(int productId);
    Task<List<Product>> GetActiveProductsAsync();
    Task<bool> ProductExistsAsync(int productId);
}

public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    private const int MaxProductsPerPage = 50;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        _logger.LogInformation("Fetching product: {ProductId}", productId);
        var product = await _repository.GetProductByIdAsync(productId);
        return product?.MapToDto();
    }
}
```

## Error Messages & Exceptions

- [ ] Exception messages are clear and actionable
- [ ] Include context: what failed, why, what to try next
- [ ] Use specific exception types (not bare `Exception`)
- [ ] Logged exceptions include stack trace
- [ ] User-facing error messages don't expose internals

**Example - GOOD**:
```csharp
// Exception - GOOD
if (productId <= 0)
    throw new ArgumentException(
        $"Product ID must be positive. Received: {productId}", 
        nameof(productId));

// Exception - BAD
if (productId <= 0)
    throw new Exception("Bad ID");

// Logging - GOOD
catch (SqlException ex)
{
    _logger.LogError(ex, 
        "Database error retrieving product: {ProductId}. Connection: {Connection}", 
        productId, "CatalogDb");
}

// Error Response - GOOD (hide internals)
return BadRequest(new { error = "Invalid product ID" });
```

## Method Design

- [ ] Methods do one thing (Single Responsibility Principle)
- [ ] Method length reasonable (aim for <30 lines)
- [ ] Parameter count reasonable (aim for <5 parameters)
- [ ] Return types consistent and predictable
- [ ] Side effects documented or minimized
- [ ] Comments only for "why", not "what" (code should be self-documenting)

**Example - GOOD**:
```csharp
// Single Responsibility
public async Task<ProductDto> GetProductAsync(int productId)
{
    var product = await _repository.GetProductByIdAsync(productId);
    return product?.MapToDto();
}

public async Task<bool> CanCreateProductAsync(string name)
    => !await _repository.ProductNameExistsAsync(name);

// Bad - Too Much Responsibility
public async Task<object> ProcessAsync(int id)
{
    var product = await _db.Products.FindAsync(id);
    product.UpdatedAt = DateTime.Now;
    product.ViewCount++;
    await _db.SaveChangesAsync();
    await _cache.SetAsync("product_" + id, product);
    await _logger.LogAsync("Product processed");
    return new { id = product.Id, name = product.Name };
}
```

## Comments & Documentation

- [ ] Comments explain "why", not "what"
- [ ] Code is self-documenting (clear names reduce need for comments)
- [ ] XML documentation on public methods: `/// <summary>`
- [ ] TODO/FIXME comments are dated and assigned if applicable
- [ ] Commented-out code is removed (use git history if needed)
- [ ] No comments stating the obvious

**Example - GOOD**:
```csharp
// WHY comment - GOOD
public async Task<Product> GetProductAsync(int id)
{
    // Cache 30 minutes to reduce database load during peak hours
    var cacheKey = $"product_{id}";
    var cached = await _cache.GetAsync<Product>(cacheKey);
    if (cached != null) return cached;

    var product = await _repository.GetByIdAsync(id);
    await _cache.SetAsync(cacheKey, product, TimeSpan.FromMinutes(30));
    return product;
}

// WHAT comment - BAD (code is self-explanatory)
public async Task<Product> GetProductAsync(int id)
{
    // Get product from cache
    var cached = await _cache.GetAsync<Product>($"product_{id}");
    // If found, return it
    if (cached != null) return cached;
    // Otherwise get from database...
}

// Good XML documentation
/// <summary>
/// Retrieves a product by its ID with caching to reduce database load.
/// </summary>
/// <param name="id">The product ID to retrieve.</param>
/// <returns>The product if found; otherwise null.</returns>
/// <remarks>Results are cached for 30 minutes.</remarks>
public async Task<Product> GetProductAsync(int id)
```

## Readability & Structure

- [ ] Consistent indentation (4 spaces, not tabs)
- [ ] Braces on own line for classes/methods (Allman style or BSD)
- [ ] Single statement on own line (no `return x ?? y;` on one line if complex)
- [ ] Logical grouping: related code together
- [ ] Using statements organized (System.*, then custom)
- [ ] No deeply nested conditions (extract to methods, use early returns)

**Example - GOOD**:
```csharp
public class ProductService
{
    // Organize: fields, constructor, public methods, private methods
    
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ProductDto> GetProductAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("ID must be positive", nameof(id));

        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        return product.MapToDto();
    }

    private ProductDto MapToDto(Product product)
        => new ProductDto { Id = product.Id, Name = product.Name };
}

// Deep Nesting - BAD
public void ProcessOrder(Order order)
{
    if (order != null)
    {
        if (order.Items.Count > 0)
        {
            foreach (var item in order.Items)
            {
                if (item.Quantity > 0)
                {
                    if (item.Price > 0)
                    {
                        // Business logic buried deep
                    }
                }
            }
        }
    }
}

// Flattened - GOOD (early returns, extracted methods)
public void ProcessOrder(Order order)
{
    if (order?.Items?.Count == 0) return;
    foreach (var item in order.Items)
        ProcessOrderItem(item);
}

private void ProcessOrderItem(OrderItem item)
{
    if (!IsValidItem(item)) return;
    // Business logic here
}

private bool IsValidItem(OrderItem item)
    => item?.Quantity > 0 && item.Price > 0;
```

## LINQ & Collections

- [ ] LINQ queries readable (use method syntax or query syntax consistently)
- [ ] No LINQ queries in loops (N+1 problem)
- [ ] Use `.AsNoTracking()` for read-only EF Core queries
- [ ] Check for null before calling `.Count()`, use `.Any()` instead
- [ ] Don't materialize large collections unnecessarily

**Example - GOOD**:
```csharp
// GOOD - Single query
var products = await _context.Products
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Take(10)
    .AsNoTracking()
    .ToListAsync();

// BAD - N+1 Problem
foreach (var order in orders)
{
    var items = await _context.OrderItems
        .Where(x => x.OrderId == order.Id)
        .ToListAsync(); // Queries N times!
}

// GOOD - Batch query
var orderIds = orders.Select(o => o.Id).ToList();
var items = await _context.OrderItems
    .Where(x => orderIds.Contains(x.OrderId))
    .ToListAsync(); // Single query

// GOOD - Check existence efficiently
if (await _context.Products.AnyAsync(p => p.Id == id))
    // ...

// BAD - Materializes whole collection
if (_context.Products.Count() > 0)
    // ...
```

## Summary

Consistent code quality makes projects maintainable, reviewable, and scalable.
