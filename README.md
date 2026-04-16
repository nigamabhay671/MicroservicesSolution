# MicroservicesSolution

A .NET 9 microservices demonstration project implementing Clean Architecture principles with three independent, containerized services: **CatalogService**, **OrderService**, and **BasketService**.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Getting Started](#getting-started)
- [Development](#development)
- [Docker Deployment](#docker-deployment)
- [Database Migrations](#database-migrations)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Technologies](#technologies)
- [**🤖 Claude Code Reviewer Skill**](#-claude-code-reviewer-skill) ⭐
- [Code Review](#code-review)

## 🎯 Overview

This solution demonstrates best practices for building enterprise-grade microservices using .NET 9:

- **Clean Architecture**: Each service follows a four-layer architecture (Domain, Application, Infrastructure, API)
- **Independently Deployable**: Services can be built, deployed, and scaled independently
- **Containerized**: Docker support for consistent local development and production deployments
- **Structured Logging**: Serilog integration for observability
- **Validation**: FluentValidation for input validation
- **Data Mapping**: AutoMapper for DTO transformations
- **Database Migrations**: EF Core migrations applied automatically on startup

## 🏗️ Architecture

### Clean Architecture Layers

Each service implements a **four-layer architecture**:

```
┌─────────────────────────┐
│     API Layer           │  → ASP.NET Core Controllers, Swagger
├─────────────────────────┤
│  Application Layer      │  → Services, DTOs, Validators, Mappers
├─────────────────────────┤
│  Domain Layer           │  → Entities, Business Logic, Enums
├─────────────────────────┤
│ Infrastructure Layer    │  → DbContext, Repositories, Data Access
└─────────────────────────┘
```

**Key Principles:**
- Domain layer has **no external dependencies** (framework-agnostic)
- Application layer orchestrates business logic
- Infrastructure layer handles data persistence
- API layer exposes endpoints and handles HTTP concerns

### Service Communication

- Services are **loosely coupled** and can operate independently
- Each service has its own database (CatalogDB, OrderDB for PostgreSQL; Redis for BasketService)
- API contracts through DTOs prevent tight coupling

## 🚀 Services

### CatalogService (Port 8000)

**Responsibilities**: Product catalog management and inventory

| Component | Details |
|-----------|---------|
| **Database** | PostgreSQL (`CatalogDb`) |
| **Port** | 8000 |
| **Key Entities** | Product |
| **Key Operations** | Get products, Create product, Update inventory |

**Example Endpoints**:
```
GET  /api/products
GET  /api/products/{id}
POST /api/products
```

### OrderService (Port 8001)

**Responsibilities**: Order processing and order item management

| Component | Details |
|-----------|---------|
| **Database** | PostgreSQL (`OrderDb`) |
| **Port** | 8001 |
| **Key Entities** | Order, OrderItem |
| **Key Operations** | Create order, Get order, List orders |

**Example Endpoints**:
```
GET  /api/orders/{id}
POST /api/orders
```

### BasketService (Port 8002)

**Responsibilities**: Shopping basket management with session-based storage

| Component | Details |
|-----------|---------|
| **Database** | Redis (in-memory cache) |
| **Port** | 8002 |
| **Key Entities** | Basket, BasketItem |
| **Cache TTL** | 30 days |
| **Key Operations** | Get basket, Update basket, Delete basket |

**Example Endpoints**:
```
GET    /api/basket/{buyerId}
POST   /api/basket
DELETE /api/basket/{buyerId}
```

## 🏁 Getting Started

### Prerequisites

- **.NET 9 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker & Docker Compose** — [Download](https://www.docker.com/products/docker-desktop)
- **PostgreSQL** (optional, if running services without Docker)
- **Redis** (optional, if running BasketService without Docker)

### Quick Start

#### Option 1: Docker Compose (Recommended)

Run all services with their dependencies:

```bash
git clone <repository-url>
cd MicroservicesSolution

docker-compose up --build
```

**Access points**:
- CatalogService API: http://localhost:8000/swagger
- OrderService API: http://localhost:8001/swagger
- BasketService API: http://localhost:8002/swagger
- PostgreSQL: localhost:5432
- Redis: localhost:6379

#### Option 2: Local Development

**1. Start dependencies** (PostgreSQL, Redis):

```bash
docker-compose up postgres basket-redis
```

**2. Run each service** (in separate terminals):

```bash
# CatalogService
dotnet run --project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj

# OrderService
dotnet run --project src/Services/OrderService/OrderService.API/OrderService.API.csproj

# BasketService
dotnet run --project src/Services/BasketService/BasketService.API/BasketService.API.csproj
```

## 🛠️ Development

### Building the Solution

```bash
dotnet build MicroservicesSolution.sln
```

### Running Tests

```bash
dotnet test MicroservicesSolution.sln
```

### Individual Service Development

```bash
cd src/Services/CatalogService/CatalogService.API
dotnet watch run
```

### Logging Configuration

All services use **Serilog** for structured logging:

```csharp
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ServiceName")
);
```

View logs in console output. For production, configure sinks (e.g., `Serilog.Sinks.File`).

## 🐳 Docker Deployment

### Build Individual Service Images

```bash
# CatalogService
docker build -f src/Services/CatalogService/CatalogService.Dockerfile \
  -t catalogservice:dev .

# OrderService
docker build -f src/Services/OrderService/OrderService.Dockerfile \
  -t orderservice:dev .

# BasketService
docker build -f src/Services/BasketService/BasketService.Dockerfile \
  -t basketservice:dev .
```

### Run Individual Service Container

```bash
docker run -p 8000:8000 catalogservice:dev
```

### Docker Compose Services

**Services defined in `docker-compose.yml`**:

- `catalogservice` — CatalogService container
- `orderservice` — OrderService container  
- `basketservice` — BasketService container
- `postgres` — Shared PostgreSQL database (separate databases per service)
- `basket-redis` — Redis instance for BasketService

**Environment variables** override `appsettings.json`:

```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Development
  ConnectionStrings__CatalogDb: "Server=postgres;..."
  ConnectionStrings__OrderDb: "Server=postgres;..."
  ConnectionStrings__Redis: "basket-redis:6379"
```

## 💾 Database Migrations

### EF Core Auto-Migration

Migrations are **automatically applied on startup** in `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();
}
```

### Create a New Migration

```bash
# CatalogService Example
dotnet ef migrations add AddProductInventory \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj \
  --startup-project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj

# OrderService Example
dotnet ef migrations add CreateOrderSchema \
  --project src/Services/OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj \
  --startup-project src/Services/OrderService/OrderService.API/OrderService.API.csproj
```

### Update Database Manually (if needed)

```bash
dotnet ef database update \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj \
  --startup-project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj
```

### Rollback Migration

```bash
dotnet ef migrations remove \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj \
  --startup-project src/Services/CatalogService/CatalogService.API/CatalogService.API.csproj
```

## 📚 API Documentation

All services expose **Swagger/OpenAPI** documentation:

- **CatalogService**: http://localhost:8000/swagger
- **OrderService**: http://localhost:8001/swagger
- **BasketService**: http://localhost:8002/swagger

Swagger is enabled in Development. To enable in Production:

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## 📁 Project Structure

```
MicroservicesSolution/
├── src/Services/
│   ├── CatalogService/
│   │   ├── CatalogService.API/           # Web API layer
│   │   │   ├── Controllers/              # API endpoints
│   │   │   ├── Program.cs                # DI & middleware setup
│   │   │   └── appsettings.json          # Configuration
│   │   ├── CatalogService.Application/   # Business logic layer
│   │   │   ├── Services/                 # Application services
│   │   │   ├── DTOs/                     # Data transfer objects
│   │   │   └── Validators/               # FluentValidation rules
│   │   ├── CatalogService.Domain/        # Core domain layer
│   │   │   └── Entities/                 # Domain entities
│   │   └── CatalogService.Infrastructure/ # Data access layer
│   │       ├── Data/                     # DbContext, repositories
│   │       └── Migrations/               # EF Core migrations
│   │
│   ├── OrderService/                     # Similar structure
│   └── BasketService/                    # Similar structure
│
├── docker-compose.yml                    # Multi-container setup
├── docker-compose.override.yml           # Development overrides
├── MicroservicesSolution.sln             # Solution file
└── README.md                             # This file
```

## 🛠️ Technologies

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 9.0 | Runtime & Framework |
| ASP.NET Core | 9.0 | Web API framework |
| Entity Framework Core | 9.0 | ORM for PostgreSQL |
| Serilog | 4.1+ | Structured logging |
| FluentValidation | 11.0+ | Input validation |
| AutoMapper | Latest | DTO mapping |
| PostgreSQL | 15+ | Relational database |
| Redis | Latest | Cache store |
| Docker | Latest | Containerization |

## 🤖 Claude Code Reviewer Skill

For detailed code review guidance and best practices, see the [Claude Code Reviewer Skill](#-claude-code-reviewer-skill) section above.

**Quick Start**:
```bash
# Review entire service
/code-reviewer src/Services/CatalogService/

# Review specific layer
/code-reviewer src/Services/OrderService/OrderService.Application/

# Review with context
Review the logging implementation in BasketService
```

**Available Checklists**:
- Clean Architecture layer compliance
- Logging and error handling
- Dependency injection patterns
- Database and migration setup
- Security and configuration
- Code quality and naming
- Async/await patterns
- Testing readiness

See `.claude/skills/code-reviewer/SKILL.md` for comprehensive guidance
/code-reviewer src/Services/CatalogService/
/code-reviewer src/Services/OrderService/
/code-reviewer src/Services/BasketService/
```

**Comprehensive Resources:**
- **Main Skill File**: `.claude/skills/code-reviewer/SKILL.md`
- **Clean Architecture Guide**: `.claude/skills/code-reviewer/references/clean-architecture-checklist.md`
- **Cross-Cut Concerns**: `.claude/skills/code-reviewer/references/cross-cut-checklist.md`
- **Code Quality Standards**: `.claude/skills/code-reviewer/references/code-quality-checklist.md`
- **Logging Patterns**: `.claude/skills/code-reviewer/references/logging-pattern.md`
- **Microservices Patterns**: `.claude/skills/code-reviewer/references/dotnet-microservices-patterns.md`
- **Review Template**: `.claude/skills/code-reviewer/templates/review-template.md`
- **Comment Examples**: `.claude/skills/code-reviewer/templates/issue-comment-template.md`

**Example Use Cases:**
1. **Before Merging** → Check if PR follows architecture standards
2. **Feature Implementation** → Validate logging, error handling, and DI
3. **New Developer** → Learn best practices from reviewer feedback
4. **Architectural Changes** → Ensure layer separation and dependency flow
5. **Refactoring** → Verify code quality improvements

**Skill Features:**
- ✅ 5-phase review procedure (Scope → Architecture → Cross-Cut → Quality → Summary)
- ✅ Interactive checklists for consistent evaluation
- ✅ Real code examples (good vs. bad patterns)
- ✅ Contextual feedback with file references
- ✅ Actionable recommendations
- ✅ .NET 9 microservices-specific guidance

## 🔍 Code Review

This project includes a comprehensive **code-reviewer skill** for validating:

- ✅ Clean Architecture compliance (layer separation, DI)
- ✅ Logging and error handling (Serilog patterns)
- ✅ Naming conventions and code quality
- ✅ Async/await patterns (no blocking calls)
- ✅ Database and persistence patterns
- ✅ Security best practices

**Invoke the reviewer** from Claude Chat:

```
/code-reviewer src/Services/CatalogService/
```

See `.claude/skills/code-reviewer/SKILL.md` for full details.

## 📖 Configuration Reference

### Connection Strings

Services use **environment variable** conventions for Docker:

```
ConnectionStrings__ServiceName → Used in Docker
ConnectionStrings:ServiceName  → Used in appsettings.json
```

**Example** in `docker-compose.yml`:

```yaml
environment:
  ConnectionStrings__CatalogDb: "Server=postgres;Port=5432;Database=CatalogDb;User Id=postgres;Password=password;"
  ConnectionStrings__Redis: "basket-redis:6379"
```

### Environment Variables

| Variable | Values | Purpose |
|----------|--------|---------|
| `ASPNETCORE_ENVIRONMENT` | Development, Production | Controls Swagger, logging |
| `ConnectionStrings__CatalogDb` | Connection string | CatalogService database |
| `ConnectionStrings__OrderDb` | Connection string | OrderService database |
| `ConnectionStrings__Redis` | Host:Port | BasketService cache |

## 🚀 CI/CD

GitHub Actions workflow available at `.github/workflows/docker-ci.yml`:

- Automatically builds services on push to `main`
- Pushes images to GitHub Container Registry (GHCR)
- Tags images as `ghcr.io/<owner>/<service>:latest`

## 📝 License

This project is part of the Claude Architect Certification learning path.

## 🤝 Contributing

When contributing:

1. Follow Clean Architecture principles (layer separation, DI)
2. Add logging using Serilog patterns
3. Use FluentValidation for input validation
4. Run `/code-reviewer` on changed files
5. Ensure async/await patterns are used for I/O operations
6. Update migrations if schema changes

## 🆘 Troubleshooting

### Docker Services Won't Start

```bash
# Check running containers
docker-compose ps

# View logs
docker-compose logs catalogservice
docker-compose logs orderservice
docker-compose logs basketservice

# Restart services
docker-compose restart
```

### Database Connection Failed

```bash
# Ensure PostgreSQL is running
docker-compose ps postgres

# Check connection string
docker-compose logs catalogservice | grep -i "connection"

# Recreate database
docker-compose down -v
docker-compose up postgres
```

### Redis Connection Issues

```bash
# Verify Redis is running
docker-compose exec basket-redis redis-cli ping

# Check Redis configuration
docker-compose logs basket-redis
```

### Migrations Not Applied

```bash
# Check migration status
dotnet ef migrations list \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj

# Force migration
dotnet ef database update --force \
  --project src/Services/CatalogService/CatalogService.Infrastructure/CatalogService.Infrastructure.csproj
```

## 📞 Support

For issues, questions, or improvements:

1. Check troubleshooting guide above
2. Review logs via `docker-compose logs`
3. Verify services are running with `docker-compose ps`
4. Check `.claude/CLAUDE.md` for detailed guidance