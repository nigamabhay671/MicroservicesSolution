# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 microservices solution demonstrating Clean Architecture principles with three independent services: CatalogService (product management), OrderService (order processing), and BasketService (shopping cart). Each service is containerized and can be deployed independently.

## Architecture

### Clean Architecture Layers
Each service follows a four-layer Clean Architecture pattern:

- **Domain Layer**: Core business entities with encapsulated business logic. Domain entities use private setters and constructor validation (see `Product.cs` for reference).
- **Application Layer**: Service interfaces, DTOs, validation rules (FluentValidation), and AutoMapper profiles. Contains business orchestration logic.
- **Infrastructure Layer**: Data access (EF Core DbContext, repositories), external service integrations, and persistence implementation.
- **API Layer**: ASP.NET Core Web API with controllers, Swagger configuration, and dependency injection setup in `Program.cs`.

### Service Details

**CatalogService** (Port 8000)
- Database: PostgreSQL (`CatalogDb`)
- Uses EF Core with code-first migrations
- Implements product catalog with inventory management
- Connection string: `ConnectionStrings__CatalogDb`

**OrderService** (Port 8001)
- Database: PostgreSQL (`OrderDb`)  
- Manages order creation and order item relationships
- Uses EF Core migrations applied automatically on startup
- Connection string: `ConnectionStrings__OrderDb`

**BasketService** (Port 8002)
- Database: Redis (in-memory cache)
- Shopping basket with session-based storage
- Uses StackExchange.Redis client with `IConnectionMultiplexer`
- Connection string: `ConnectionStrings__Redis`

### Database Migrations
Services automatically apply pending migrations on startup via:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
    db.Database.Migrate();
}
```

## Development Commands

### Building and Running

**Build entire solution:**
```bash
dotnet build MicroservicesSolution.sln
```

**Run individual service:**
```bash
dotnet run --project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj
```

**Docker Compose (all services + dependencies):**
```bash
docker-compose up --build
```

### Database Migrations

**Create a new migration:**
```bash
# CatalogService example
dotnet ef migrations add <MigrationName> \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj \
  --startup-project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj

# OrderService example  
dotnet ef migrations add <MigrationName> \
  --project src/Services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj \
  --startup-project src/Services/OrderService/OrderService.API/OrderService.API.csproj
```

**Update database manually (if needed):**
```bash
dotnet ef database update \
  --project src/Services/<ServiceName>/<ServiceName>.Infrastructure/<ServiceName>.Infrastructure.csproj \
  --startup-project src/Services/<ServiceName>/<ServiceName>.API/<ServiceName>.API.csproj
```

### Docker

**Build individual service image:**
```bash
docker build -f src/Services/CatalogService/CatalogService.Dockerfile -t catalogservice:dev .
docker build -f src/Services/OrderService/OrderService.Dockerfile -t orderservice:dev .
docker build -f src/Services/BasketService/BasketService.Dockerfile -t basketservice:dev .
```

**Run individual service container:**
```bash
docker run -p 8000:8000 catalogservice:dev
```

## Service Dependencies

- **postgres**: Shared PostgreSQL instance for CatalogService and OrderService (separate databases)
- **basket-redis**: Redis instance exclusively for BasketService
- All services expose Swagger UI at `http://localhost:<port>/swagger`
- Health check endpoints available at `/health` (CatalogService)

## Key Technologies

- **.NET 9**: Target framework
- **Entity Framework Core**: ORM with PostgreSQL provider (Npgsql)
- **AutoMapper**: Object-to-object mapping for DTOs
- **FluentValidation**: Input validation with `CreateProductValidator` pattern
- **Swagger/OpenAPI**: API documentation (enabled in Development)
- **CORS**: Configured with "AllowAll" policy for development

## Project Structure Pattern

```
src/Services/<ServiceName>/
  ├── <ServiceName>.API/          # Web API layer
  │   ├── Controllers/            # API endpoints
  │   ├── Program.cs              # Startup configuration
  │   └── appsettings.json        # Configuration
  ├── <ServiceName>.Application/  # Business logic layer
  │   ├── Services/               # Application services
  │   ├── DTOs/                   # Data transfer objects
  │   └── Validators/             # FluentValidation rules
  ├── <ServiceName>.Domain/       # Core domain layer
  │   └── Entities/               # Domain entities
  └── <ServiceName>.Infrastructure/ # Data access layer
      ├── Data/                   # DbContext and repositories
      └── Migrations/             # EF Core migrations
```

## CI/CD

- GitHub Actions workflow: `.github/workflows/docker-ci.yml`
- Automatically builds and pushes Docker images to GitHub Container Registry (GHCR)
- Triggered on pushes to `main` branch and pull requests
- Services tagged as `ghcr.io/<owner>/catalogservice:latest` and `ghcr.io/<owner>/orderservice:latest`

## Configuration Conventions

- Connection strings use double underscore notation for Docker environment variables: `ConnectionStrings__CatalogDb`
- Services use Kestrel configured to `ListenAnyIP(<port>)` for containerization
- All services use `ASPNETCORE_ENVIRONMENT` to control behavior (Development/Production)
