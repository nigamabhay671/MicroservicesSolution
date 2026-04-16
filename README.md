# claude-architect

**Enterprise-Grade .NET 9 Microservices with AI-Powered Code Review** — A production-ready demonstration of Clean Architecture principles featuring three independent, containerized services (**CatalogService**, **OrderService**, **BasketService**) with integrated Claude AI code reviewer.

> 🤖 **AI-Assisted Architecture Validation** — Features an intelligent Claude Code Reviewer Skill that provides context-aware code reviews, architecture validation, and continuous quality assurance throughout the development lifecycle.

---

## 🤖 Claude Code Reviewer Skill — AI-Powered Architecture Validation

### What is Claude Code?

**Claude Code** is an AI-powered development assistant integrated directly into VS Code that helps you write, review, and refactor code intelligently. Unlike traditional linters, Claude Code understands:

- **Business Logic** — Why code exists, not just syntax
- **Architecture Patterns** — Clean Architecture, microservices, DDD principles
- **Context** — Your project structure and conventions
- **Best Practices** — Industry standards for .NET 9, async patterns, security

### What is the Code Reviewer Skill?

A **Skill** is a specialized workflow bundled with Claude Code that you can invoke with `/skill-name`. The **code-reviewer skill** is a comprehensive, reusable code review framework specifically designed for this .NET 9 microservices project.

**Invoke it in Claude Chat**:
```bash
/code-reviewer src/Services/CatalogService/
/code-reviewer src/Services/OrderService/OrderService.Application/
Review the logging implementation in BasketService
```

### 🎯 What It Does

This skill performs **intelligent, holistic code reviews** across multiple dimensions:

| Dimension | Coverage |
|-----------|----------|
| 🏗️ **Architecture** | Clean Architecture layer separation, dependency flow, layer violations |
| 🔍 **Code Quality** | Naming conventions, readability, method complexity, LINQ patterns |
| 📝 **Logging** | Serilog structured logging validation, log levels, sensitive data checking |
| ⚡ **Async Patterns** | Async/await validation, blocking call detection, deadlock prevention |
| 🗄️ **Database** | EF Core migrations, DbContext setup, repository patterns, N+1 queries |
| 🔗 **Dependency Injection** | DI registration, lifecycle management, interface patterns |
| 🚨 **Error Handling** | Exception handling completeness, context preservation, logging |
| 🔒 **Security** | Hardcoded credentials, CORS configuration, sensitive data in logs |

### 💡 How It Works

1. **Analysis Phase** — Examines code for architectural and quality issues
2. **Structured Evaluation** — Runs against comprehensive checklists
3. **Contextual Feedback** — Provides file-specific, line-referenced recommendations
4. **Actionable Guidance** — Shows good vs. bad patterns with examples
5. **Learning Resource** — Helps teams understand and adopt best practices

### 📚 Resources Available

The skill includes **8 comprehensive reference documents**:

- ✅ **SKILL.md** — Main skill procedure and phase-by-phase review workflow
- ✅ **clean-architecture-checklist.md** — Domain, Application, Infrastructure, API layer validation
- ✅ **cross-cut-checklist.md** — Logging, DI, database, security, async patterns
- ✅ **code-quality-checklist.md** — Naming, style, comments, LINQ, method design
- ✅ **logging-pattern.md** — Serilog setup, structured logging guide with examples
- ✅ **dotnet-microservices-patterns.md** — .NET 9 patterns (good vs. bad) specific to this solution
- ✅ **review-template.md** — Structured format for consistent review reporting
- ✅ **issue-comment-template.md** — PR comment examples (praise, suggestions, issues, questions)

**Location**: `.claude/skills/code-reviewer/`

### 🎓 Learning Outcomes

When using the code-reviewer skill, you'll learn:

- ✅ How to structure .NET 9 microservices following Clean Architecture
- ✅ Logging best practices with Serilog and structured logging
- ✅ Dependency injection patterns and lifecycle management
- ✅ Async/await patterns and common pitfalls
- ✅ Security considerations for microservices
- ✅ Database patterns with EF Core and repository pattern
- ✅ Code quality standards for enterprise applications

### 🚀 Quick Start — Using Claude Code Reviewer

**1. Review an entire service:**
```bash
/code-reviewer src/Services/CatalogService/
```

**2. Review a specific layer:**
```bash
/code-reviewer src/Services/OrderService/OrderService.Application/
```

**3. Review with narrative context:**
```bash
Review the logging implementation in BasketService to ensure it follows Serilog patterns
```

**4. Review before merging:**
```bash
Review this PR for Clean Architecture compliance and logging completeness
```

The skill will provide a structured review covering architecture, code quality, cross-cutting concerns, and actionable recommendations.

---

## 📋 Table of Contents

- [🤖 Claude Code & Skill](#-claude-code-reviewer-skill--ai-powered-architecture-validation) ⭐ **START HERE**
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
- [Code Review Details](#code-review-details)
- [Configuration Reference](#-configuration-reference)
- [Contributing](#-contributing)
- [Troubleshooting](#-troubleshooting)

## 🎯 Project Overview

**claude-architect** is a showcase project demonstrating enterprise-grade microservices development with AI-assisted quality assurance:

### Technical Excellence
- ✅ **Clean Architecture**: Four-layer pattern (Domain → Application → Infrastructure → API)
- ✅ **Independently Deployable**: Each service operates autonomously with its own data store
- ✅ **Containerized & Orchestrated**: Docker Compose for local development & production deployment
- ✅ **Structured Logging**: Serilog integration for comprehensive observability
- ✅ **Type-Safe Validation**: FluentValidation for robust input handling
- ✅ **Data Mapping**: AutoMapper for clean DTO transformations
- ✅ **Database Migrations**: EF Core with automatic schema versioning

### AI-Assisted Development
- 🤖 **Claude Code Reviewer**: Intelligent code review skill with architectural validation
- 📚 **Comprehensive Guidelines**: 8 reference documents covering best practices
- 🎓 **Learning-Focused**: PR templates, checklists, and pattern examples
- 🔍 **Multi-Dimensional Review**: Architecture, logging, async, DB, DI, security, quality

### Why This Project Stands Out
- **Production-Ready**: Not a toy project—uses enterprise patterns (DDD, repository pattern, dependency injection)
- **AI-Enhanced**: Demonstrates integration of AI tools in development workflow
- **Educational**: Each layer includes validation and learning resources
- **Scalable**: Service-oriented architecture supports growth

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
git clone https://github.com/<owner>/claude-architect.git
cd claude-architect

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
dotnet build claude-architect.sln
```

### Running Tests

```bash
dotnet test claude-architect.sln
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
claude-architect/
├── src/Services/
│   ├── CatalogService/                   # Product Catalog Management
│   │   ├── CatalogService.API/           # REST API Layer
│   │   │   ├── Controllers/              # HTTP Endpoints
│   │   │   ├── Program.cs                # DI & Middleware Configuration
│   │   │   └── appsettings.json          # Service Configuration
│   │   ├── CatalogService.Application/   # Business Logic Layer
│   │   │   ├── Services/                 # Application Services
│   │   │   ├── DTOs/                     # Data Transfer Objects
│   │   │   └── Validators/               # FluentValidation Rules
│   │   ├── CatalogService.Domain/        # Domain Layer (Core Business)
│   │   │   └── Entities/                 # Domain Entities
│   │   └── CatalogService.Infrastructure/# Data Access Layer
│   │       ├── Data/                     # DbContext, Repositories
│   │       └── Migrations/               # EF Core Migrations
│   │
│   ├── OrderService/                     # Order Processing Service
│   │   └── [Similar Clean Architecture]  # (See CatalogService pattern)
│   │
│   └── BasketService/                    # Shopping Basket Service
│       └── [Similar Clean Architecture]  # Redis-backed service
│
├── .claude/                              # Claude Customization
│   ├── CLAUDE.md                         # Project-specific guidance
│   ├── skills/code-reviewer/             # AI Code Review Skill
│   │   ├── SKILL.md                      # Skill definition
│   │   ├── references/                   # Comprehensive guides
│   │   └── templates/                    # Review templates
│   └── .instructions.md                  # Agent instructions
│
├── docker-compose.yml                    # Production-like configuration
├── docker-compose.override.yml           # Development overrides
├── claude-architect.sln                  # Visual Studio Solution
├── README.md                             # Project Documentation
└── .github/workflows/                    # CI/CD Pipelines
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

For detailed information on how to use Claude Code and the code-reviewer skill, see the [**Claude Code Reviewer Skill**](#-claude-code-reviewer-skill--ai-powered-architecture-validation) section at the top of this README.

**Quick Usage**:
```bash
/code-reviewer src/Services/CatalogService/
/code-reviewer src/Services/OrderService/
/code-reviewer src/Services/BasketService/
```

## 🔍 Code Review Details

The code-reviewer skill validates across **8 key dimensions**:

| Dimension | What It Checks |
|-----------|----------------|
| 🏗️ **Architecture** | Layer separation, dependency flow, SOLID principles |
| 📝 **Logging** | Serilog setup, structured logging, log levels |
| ⚡ **Async Patterns** | Async/await correctness, blocking calls, deadlocks |
| 🗄️ **Database** | EF Core, migrations, repositories, N+1 queries |
| 🔗 **Dependency Injection** | DI registration, lifecycle, interface patterns |
| 🚨 **Error Handling** | Exception handling, context preservation, logging |
| 🔒 **Security** | Hardcoded credentials, CORS, sensitive data |
| 📊 **Code Quality** | Naming, complexity, readability, LINQ patterns |

**All resources** (checklists, templates, patterns) are located in:
```
.claude/skills/code-reviewer/
├── SKILL.md                              # Main procedure
├── references/                          # Comprehensive guides
│   ├── clean-architecture-checklist.md
│   ├── cross-cut-checklist.md
│   ├── code-quality-checklist.md
│   ├── logging-pattern.md
│   └── dotnet-microservices-patterns.md
└── templates/                           # Ready-to-use formats
    ├── review-template.md
    └── issue-comment-template.md
```

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

This project uses **Claude Code Reviewer Skill** for guidance. Follow these principles:

**Before Submitting a PR:**

1. ✅ **Run code review** — Use the Claude code-reviewer skill:
   ```bash
   /code-reviewer src/Services/[YourService]/
   ```

2. ✅ **Follow Clean Architecture** — Ensure layer separation and DI
   - Domain: No framework dependencies
   - Application: Services, DTOs, validators
   - Infrastructure: DbContext, repositories
   - API: Thin controllers, endpoints only

3. ✅ **Add structured logging** — Use Serilog patterns:
   ```csharp
   _logger.LogInformation("Operation: {Parameter}", value);
   ```

4. ✅ **Use FluentValidation** — Input validation in Application layer:
   ```csharp
   builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
   ```

5. ✅ **Async/await patterns** — No blocking calls:
   ```csharp
   // ✅ Good
   var result = await _repository.GetAsync(id);
   
   // ❌ Bad  
   var result = _repository.GetAsync(id).Result;
   ```

6. ✅ **Database migrations** — Update if schema changes:
   ```bash
   dotnet ef migrations add [MigrationName] --project [Infrastructure]
   ```

7. ✅ **Security best practices**:
   - No hardcoded credentials
   - Connection strings from configuration
   - No sensitive data in logs

**Review Checklist**:
- [ ] Code passes `/code-reviewer` skill review
- [ ] All methods have logging entry/exit
- [ ] Async operations are properly awaited
- [ ] Dependencies injected (not static)
- [ ] Migrations created for schema changes
- [ ] Error handling with context
- [ ] No blocking calls (`.Result`, `.Wait()`)
- [ ] DTOs separate from domain entities

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

## 📞 Support & Learning Resources

**For code review & best practices guidance:**
- 🤖 Use `/code-reviewer` skill in Claude Chat (see [top of README](#-claude-code-reviewer-skill--ai-powered-architecture-validation))
- 📖 Read `.claude/CLAUDE.md` for project-specific guidance
- 📚 Review `.claude/skills/code-reviewer/references/` for checklists and patterns

**For troubleshooting:**

1. Check troubleshooting guide below
2. Review logs via `docker-compose logs`
3. Verify services are running with `docker-compose ps`
4. Check `.claude/CLAUDE.md` for detailed guidance

**For learning architecture patterns:**
- See `.claude/skills/code-reviewer/references/clean-architecture-checklist.md`
- See `.claude/skills/code-reviewer/references/dotnet-microservices-patterns.md`
- See `.claude/skills/code-reviewer/templates/review-template.md` for review format