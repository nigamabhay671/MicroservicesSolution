# Cross-Cut Concerns Checklist

Review these horizontal concerns that span across all layers: logging, error handling, dependency injection, database configuration, security, and testing readiness.

## Logging & Error Handling

- [ ] All public methods log entry: `_logger.LogInformation("Operation: {Param}", value)`
- [ ] All operations log success/failure with context
- [ ] Async operations wrapped in try-catch with logging
- [ ] Exception type specified (ArgumentNullException, InvalidOperationException, not bare Exception)
- [ ] Log parameters use structured format: `{ParameterName}` (not string concatenation)
- [ ] Log levels correct:
  - **Information**: Business events (created, updated, deleted, started)
  - **Debug**: Detailed flow, intermediate values
  - **Warning**: Unexpected but recoverable (not found, validation failure)
  - **Error**: Exceptions, operations that failed
- [ ] No sensitive data logged (passwords, API keys, tokens, SSNs)
- [ ] No PII logged unless absolutely necessary and justified

**Example - GOOD**:
```csharp
public async Task<Product> CreateProductAsync(CreateProductRequest request)
{
    _logger.LogInformation("Creating product: {ProductName}", request.Name);
    try
    {
        var product = new Product(request.Name, request.Price);
        var created = await _repository.AddAsync(product);
        _logger.LogDebug("Product created with ID: {ProductId}", created.Id);
        return created;
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning(ex, "Product creation validation failed: {ProductName}", request.Name);
        throw;
    }
}
```

## Dependency Injection

- [ ] All services registered in Program.cs with correct lifetime:
  - **Transient**: Created new each time (stateless utilities)
  - **Scoped**: Created once per request (DbContext, repositories)
  - **Singleton**: Created once for application (configuration, IConnectionMultiplexer)
- [ ] ILogger<T> injected (not static, not ServiceLocator pattern)
- [ ] Interfaces injected, not concrete implementations
- [ ] DbContext registered as scoped
- [ ] IConnectionMultiplexer registered as singleton
- [ ] No circular dependencies

**Example - GOOD**:
```csharp
// Program.cs DI Setup
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductAppService, ProductAppService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("Redis"), true);
    return ConnectionMultiplexer.Connect(config);
});

// Constructor Injection
public class ProductController
{
    private readonly IProductAppService _service;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductAppService service, ILogger<ProductController> logger)
    {
        _service = service;
        _logger = logger;
    }
}
```

## Database & Migrations

- [ ] DbContext configured in Program.cs with connection string from configuration
- [ ] Connection string uses environment variable convention: `ConnectionStrings__ServiceName`
- [ ] Migrations automatically applied on startup: `app.Services.MigrateDatabase()`
- [ ] Migration files properly named and sequenced
- [ ] No `DateTime.Now` hardcoded; use database server time or app time consistently
- [ ] All async operations use `await` (no `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`)
- [ ] Connection pooling configured (EF Core: connection string params; Redis: singleton IConnectionMultiplexer)
- [ ] Error handling for connection failures

**Example - GOOD**:
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    try
    {
        db.Database.Migrate();
        logger.LogInformation("Database migrations completed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed");
        throw;
    }
}

app.Run();
```

**Connection String (appsettings.json)**:
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Server=postgres;Port=5432;Database=CatalogDb;User Id=postgres;Password=password;"
  }
}
```

**Enviroment Override (docker-compose.yml)**:
```yaml
environment:
  ConnectionStrings__CatalogDb: "Server=postgres;Port=5432;Database=CatalogDb;User Id=postgres;Password=password;"
```

## Security & Configuration

- [ ] No hardcoded credentials in source code
- [ ] Sensitive data (connection strings, API keys) in Configuration or environment variables
- [ ] CORS policy defined (check "AllowAll" is development-only or appropriately restrictive)
- [ ] Authentication/authorization configured if required
- [ ] Input validation on all API endpoints via FluentValidation
- [ ] appsettings.json vs. appsettings.Development.json separation
- [ ] Docker environment variables override appsettings values
- [ ] No secrets in appsettings (use User Secrets in Development, Key Vault in Production)

**Example - GOOD**:
```csharp
// Program.cs - Secure CORS
builder.Services.AddCors(options =>
{
    if (app.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    }
    else
    {
        options.AddPolicy("RestrictedCors", policy =>
            policy.WithOrigins("https://trusted-domain.com")
                  .WithMethods("GET", "POST")
                  .WithHeaders("Authorization", "Content-Type"));
    }
});

// Program.cs - Configuration
var connectionString = builder.Configuration.GetConnectionString("CatalogDb")
    ?? throw new InvalidOperationException("Connection string 'CatalogDb' not found");
```

## Async/Performance

- [ ] All I/O operations async (database, Redis, HTTP calls)
- [ ] No blocking calls: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`
- [ ] Connection pooling configured:
  - EF Core: default pool size 128
  - Redis: IConnectionMultiplexer singleton
- [ ] Batch operations instead of N+1 queries when possible
- [ ] Select appropriate repository/cache strategy
- [ ] No unnecessary allocations or LINQ queries in loops

**Example - GOOD**:
```csharp
// Async/Await - GOOD
public async Task<List<Product>> GetProductsAsync()
{
    return await _context.Products.ToListAsync();
}

// Blocking - BAD
public List<Product> GetProducts()
{
    return _context.Products.ToList().Result; // BLOCKS!
}

// N+1 Query - BAD
foreach (var order in orders)
{
    var items = await _context.OrderItems
        .Where(x => x.OrderId == order.Id)
        .ToListAsync(); // N queries!
}

// Batch Query - GOOD
var items = await _context.OrderItems
    .Where(x => orderIds.Contains(x.OrderId))
    .ToListAsync(); // 1 query!
```

## Testing Readiness

- [ ] Dependencies injectable (interfaces, not static methods)
- [ ] No service locator pattern (not calling `GetService()` in business logic)
- [ ] Pure functions where possible (deterministic, no side effects)
- [ ] Entities independently testable (no EF Core Required)
- [ ] Repositories mockable (interface-based)
- [ ] Services mockable (testable separation)

**Example - GOOD**:
```csharp
// Testable - Good
public class OrderService
{
    private readonly IOrderRepository _repo;
    public OrderService(IOrderRepository repo) => _repo = repo;
    public async Task<Order> CreateOrderAsync(CreateOrderRequest req) => ...
}

// Hard to Test - Bad
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderRequest req)
    {
        var repo = ServiceLocator.GetService<IOrderRepository>(); // ServiceLocator!
        return ...
    }
}
```

## Summary

All concerns should be present and consistent across all layers. If one area is weak, review the entire service systematically.
