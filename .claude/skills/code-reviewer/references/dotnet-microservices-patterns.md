# .NET 9 Microservices Patterns

Quick reference for common patterns and anti-patterns specific to the solution architecture.

## Service Registration (Program.cs)

### ✅ GOOD Pattern

```csharp
var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((context, config) => 
    config.MinimumLevel.Debug()
          .WriteTo.Console()
          .Enrich.FromLogContext()
          .Enrich.WithProperty("Service", "CatalogService"));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Database
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDb")));

// Validation & Mapping
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
builder.Services.AddAutoMapper(typeof(ProductProfile));

// Services
builder.Services.AddScoped<IProductAppService, ProductAppService>();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
    options.ListenAnyIP(8000)); // Containerized port

var app = builder.Build();
app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("CatalogService starting on port 8000");

app.MapControllers();
app.Run();
```

### ❌ BAD Pattern

```csharp
// Missing Serilog - no structured logging
var builder = WebApplication.CreateBuilder(args);

// Hardcoded connection string - security risk
var connectionString = "Server=localhost;Database=CatalogDb;User=admin;Password=123456;";
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

// No auto-migration - manual setup required
builder.Services.AddScoped<IProductAppService, ProductAppService>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

## Entity Design

### ✅ GOOD - Domain-Driven Design

```csharp
public class Order
{
    public int Id { get; private set; }
    public string BuyerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public Order(string buyerId)
    {
        if (string.IsNullOrWhiteSpace(buyerId))
            throw new ArgumentException("BuyerId required", nameof(buyerId));
        
        BuyerId = buyerId;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    // Domain behavior encapsulated, not anemic
    public void AddItem(OrderItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to non-pending order");
        
        _items.Add(item);
    }

    public void Confirm()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Order must have items to confirm");
        
        Status = OrderStatus.Confirmed;
    }
}
```

### ❌ BAD - Anemic Model

```csharp
public class Order
{
    public int Id { get; set; }
    public string BuyerId { get; set; }
    public string Status { get; set; }  // String instead of enum
    public DateTime? CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    
    // No constructor validation, no behavior
    // All business logic would be in service/controller
}
```

## Controller Design

### ✅ GOOD - Thin Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderAppService _appService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderAppService appService, ILogger<OrdersController> logger)
    {
        _appService = appService;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetOrder(int orderId)
    {
        _logger.LogInformation("Getting order: {OrderId}", orderId);
        
        try
        {
            var order = await _appService.GetOrderAsync(orderId);
            if (order == null)
                return NotFound();
            
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order: {OrderId}", orderId);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Creating order for buyer: {BuyerId}", request.BuyerId);
        
        var order = await _appService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
    }
}
```

### ❌ BAD - Fat Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderDbContext _context;

    public OrdersController(OrderDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Business logic in controller - BAD!
        var order = new Order { BuyerId = request.BuyerId, CreatedAt = DateTime.Now };
        
        foreach (var item in request.Items)
        {
            // N+1 query - BAD!
            var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null)
                return BadRequest("Product not found");
            
            order.Items.Add(new OrderItem { ProductId = item.ProductId });
        }
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        return Ok(order); // Returns domain entity - BAD!
    }
}
```

## Repository & Data Access

### ✅ GOOD - Repository Pattern

```csharp
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(int id);
    Task<List<Order>> GetByBuyerAsync(string buyerId);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(int id);
}

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public async Task<Order> GetByIdAsync(int id)
    {
        _logger.LogDebug("Fetching order from database: {OrderId}", id);
        
        try
        {
            var order = await _context.Orders
                .Include(o => o.Items)  // Eager load items
                .AsNoTracking()         // Read-only query
                .FirstOrDefaultAsync(o => o.Id == id);
            
            if (order == null)
                _logger.LogDebug("Order not found: {OrderId}", id);
            
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error fetching order: {OrderId}", id);
            throw;
        }
    }

    public async Task<Order> AddAsync(Order order)
    {
        _logger.LogInformation("Adding order for buyer: {BuyerId}", order.BuyerId);
        
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Order added with ID: {OrderId}", order.Id);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding order for buyer: {BuyerId}", order.BuyerId);
            throw;
        }
    }
}
```

## Cache Pattern (Redis)

### ✅ GOOD - Redis Caching

```csharp
public class BasketService
{
    private readonly IBasketRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<BasketService> _logger;
    private const string CacheKeyPrefix = "basket_";
    private const int CacheExpiryMinutes = 30;

    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        var cacheKey = $"{CacheKeyPrefix}{buyerId}";
        
        _logger.LogDebug("Checking cache for basket: {CacheKey}", cacheKey);
        var cached = await _cache.GetAsync<Basket>(cacheKey);
        
        if (cached != null)
        {
            _logger.LogDebug("Cache hit for basket: {BuyerId}", buyerId);
            return cached;
        }
        
        _logger.LogDebug("Cache miss for basket: {BuyerId}, fetching from storage", buyerId);
        var basket = await _repository.GetBasketAsync(buyerId);
        
        if (basket != null)
        {
            await _cache.SetAsync(cacheKey, basket, 
                TimeSpan.FromMinutes(CacheExpiryMinutes));
            _logger.LogDebug("Cached basket: {BuyerId}", buyerId);
        }
        
        return basket;
    }

    public async Task InvalidateBasketCacheAsync(string buyerId)
    {
        var cacheKey = $"{CacheKeyPrefix}{buyerId}";
        await _cache.RemoveAsync(cacheKey);
        _logger.LogDebug("Cache invalidated for basket: {BuyerId}", buyerId);
    }
}
```

### ❌ BAD - Tight Coupling to Redis

```csharp
public class BasketService
{
    private readonly IConnectionMultiplexer _redis;

    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        // Direct Redis dependency - hard to test
        var db = _redis.GetDatabase();
        var data = await db.StringGetAsync(buyerId);
        
        if (data.IsNullOrEmpty)
            return null;
        
        // Serialization logic directly in service
        return JsonConvert.DeserializeObject<Basket>(data.ToString());
    }
}
```

## Validation Pattern

### ✅ GOOD - FluentValidation

```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator(IOrderRepository orderRepository)
    {
        RuleFor(x => x.BuyerId)
            .NotEmpty().WithMessage("BuyerId is required")
            .MinimumLength(3).WithMessage("BuyerId must be at least 3 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.All(i => i.Quantity > 0))
            .WithMessage("Item quantity must be positive");

        RuleFor(x => x)
            .MustAsync(async (req, ct) =>
            {
                // Async validation (check if products exist)
                var invalid = req.Items.Where(i => !await orderRepository
                    .ProductExistsAsync(i.ProductId)).Any();
                return !invalid;
            })
            .WithMessage("One or more products not found");
    }
}

// Applied in Program.cs
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();
```

### ❌ BAD - Manual Validation

```csharp
public class OrdersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        // Manual validation - error prone, repeated
        if (string.IsNullOrEmpty(request.BuyerId))
            return BadRequest("BuyerId required");
        
        if (request.BuyerId.Length < 3)
            return BadRequest("BuyerId too short");
        
        if (!request.Items.Any())
            return BadRequest("Must have items");
        
        // More validation...
        // This repeats in every endpoint!
    }
}
```

## Async/Await Pattern

### ✅ GOOD

```csharp
public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
{
    var product = new Product(request.Name, request.Price);
    var created = await _repository.AddAsync(product);  // ✅ Await
    return created.MapToDto();
}

public async Task<List<ProductDto>> GetProductsAsync()
{
    var products = await _context.Products.ToListAsync();  // ✅ Await
    return products.Select(p => p.MapToDto()).ToList();
}
```

### ❌ BAD - Blocking Calls

```csharp
public ProductDto CreateProduct(CreateProductRequest request)
{
    var product = new Product(request.Name, request.Price);
    var created = _repository.AddAsync(product).Result;  // ❌ BLOCKS!
    return created.MapToDto();
}

public List<ProductDto> GetProducts()
{
    var products = _context.Products.ToListAsync().GetAwaiter().GetResult();  // ❌ BLOCKS!
    return products.Select(p => p.MapToDto()).ToList();
}
```

---

These patterns represent best practices for the microservices solution. Use them as reference when reviewing and implementing features.
