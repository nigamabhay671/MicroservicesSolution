# Clean Architecture Checklist

Use this checklist when reviewing code for Clean Architecture compliance across the Domain, Application, Infrastructure, and API layers.

## Domain Layer Review

**File locations**: `[ServiceName].Domain/Entities/`

- [ ] Entities have private setters (prevent invalid state mutations)
- [ ] Constructor includes parameter validation with meaningful error messages
- [ ] Business logic is encapsulated in entity (not just getters/setters)
- [ ] No references to external NuGet packages (EF Core, AutoMapper, etc.)
- [ ] No DbContext, ILogger, or framework dependencies
- [ ] Enums/Events properly defined if used
- [ ] Entity relationships clearly defined (one-to-many, foreign keys)

**Example - GOOD**:
```csharp
public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (price < 0)
            throw new ArgumentException("Price cannot be negative");
        
        Name = name;
        Price = price;
    }
}
```

## Application Layer Review

**File locations**: `[ServiceName].Application/`

- [ ] Service interfaces defined before implementations
- [ ] DTOs separate from domain entities (no entity leakage to API)
- [ ] FluentValidation validators created for request DTOs
- [ ] AutoMapper profiles map domains to DTOs properly
- [ ] Application services contain business orchestration logic
- [ ] No direct DbContext usage (goes through repositories)
- [ ] No direct controller logic (separated into services)

**Example - GOOD**:
```csharp
// Interface
public interface IProductAppService
{
    Task<ProductDto> GetProductAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
}

// DTO (separate from Product entity)
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Validator
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(3);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Mapper Profile
public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductRequest, Product>();
    }
}
```

## Infrastructure Layer Review

**File locations**: `[ServiceName].Infrastructure/`

- [ ] DbContext inherits from correct name (e.g., `CatalogDbContext`)
- [ ] OnModelCreating configures relationships and constraints
- [ ] Repositories implement application layer interfaces
- [ ] Repository methods are async (Task, not synchronous)
- [ ] Connection string retrieved from configuration (not hardcoded)
- [ ] EF Core migrations exist and are named meaningfully
- [ ] Dependency injection extensions in `ServiceCollectionExtensions.cs`

**Example - GOOD**:
```csharp
// DbContext
public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) 
        : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });
    }
}

// Repository
public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;

    public async Task<Product> GetByIdAsync(int id)
        => await _context.Products.FindAsync(id);
}

// DI Extension
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("CatalogDb")));
        
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}
```

## API Layer Review

**File locations**: `[ServiceName].API/`

- [ ] Controllers are thin (delegate to services, not business logic)
- [ ] Controllers have consistent `[ApiController]` and `[Route("api/[controller]")]`
- [ ] All dependencies injected via constructor (not static)
- [ ] HTTP methods correct (GET, POST, PUT, DELETE)
- [ ] Status codes appropriate (200, 201, 204, 400, 404, 500)
- [ ] Program.cs registers all services and middleware
- [ ] Swagger/OpenAPI properly configured
- [ ] CORS policy defined appropriately

**Example - GOOD**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductAppService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductAppService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        _logger.LogInformation("Getting product: {ProductId}", id);
        var product = await _service.GetProductAsync(id);
        if (product == null)
            return NotFound();
        return Ok(product);
    }
}
```

**DI Setup - Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(ProductProfile));
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
builder.Services.AddCatalogInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

## Common Violations

| Layer | Violation | Reason| Fix |
|-------|-----------|-------|-----|
| Domain | References EF Core | Domain should be framework-agnostic | Move to Infrastructure |
| Domain | No constructor validation | Allow invalid entities | Add validation in constructor |
| Application | Uses DbContext directly | Violates repository pattern | Create repository interface |
| Application | Returns domain entities to API | Breaks abstraction | Create DTOs, map via AutoMapper |
| Infrastructure | Hardcoded connection string | Security risk, not portable | Use Configuration |
| API | Business logic in controller | Violates single responsibility | Extract to service class |
| API | No logging | Debugging difficult | Add ILogger<T> injection |
