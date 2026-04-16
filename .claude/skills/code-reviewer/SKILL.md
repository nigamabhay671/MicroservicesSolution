---
name: code-reviewer
description: 'Review .NET 9 microservices code for architecture compliance, design patterns, security, and quality. Use when: reviewing pull requests, validating Clean Architecture layers, checking logging/error handling, verifying DI setup, security best practices.'
argument-hint: 'File or folder path to review'
user-invocable: true
disable-model-invocation: false
---

# Code Reviewer

Automated code review for .NET 9 microservices following Clean Architecture principles. Validates layer separation, dependency injection patterns, error handling, logging, security, and database migrations.

## When to Use

- **Pull Request Reviews**: Comprehensive review before merging
- **Feature Implementation**: Validate logging, error handling, and data access
- **Architectural Changes**: Verify layer compliance and dependency flow
- **Cross-Cut Concerns**: Check CORS, security, configuration patterns
- **Database Changes**: Review migrations, connection strings, DbContext setup
- **Service Integration**: Validate inter-service communication patterns

## Architecture Context

This solution implements Clean Architecture across three services (CatalogService, OrderService, BasketService):

- **Domain Layer**: Business entities with encapsulated logic, private setters, constructor validation
- **Application Layer**: Service interfaces, DTOs, FluentValidation rules, AutoMapper profiles
- **Infrastructure Layer**: EF Core DbContext/repositories, Redis clients, data persistence
- **API Layer**: Controllers, Swagger config, dependency injection in Program.cs

Each service can be reviewed independently or compared for consistency.

## Review Procedure

### Phase 1: Determine Scope & Context

1. Identify which service(s) are affected: CatalogService, OrderService, or BasketService
2. Determine layer(s): API, Application, Domain, or Infrastructure
3. Check git diff to understand what changed
4. Review related files: entities, DTOs, controllers, services, migrations
5. Note any cross-service impacts (e.g., API contracts)

### Phase 2: Architectural Compliance

Review against [Clean Architecture Checklist](./references/clean-architecture-checklist.md):

1. **Domain Layer** 
   - Entities use private setters with constructor validation
   - No dependencies on external frameworks
   - Business logic is encapsulated, not anemic

2. **Application Layer**
   - DTOs defined for API contracts (no domain entities leaked)
   - Service interfaces before implementations
   - FluentValidation rules applied consistently
   - AutoMapper profiles for entity-to-DTO mappings

3. **Infrastructure Layer**
   - DbContext inherits correctly (CatalogDbContext, OrderDbContext)
   - Repositories implement application interfaces
   - Migrations are properly named and sequenced
   - Connection strings use environment variable conventions

4. **API Layer**
   - Controllers thin (business logic in services)
   - Dependency injection configured in Program.cs
   - Consistent routing conventions (`[ApiController]`, `[Route("api/[controller]")]`)
   - Swagger/OpenAPI properly configured

### Phase 3: Cross-Cutting Concerns

Review [Cross-Cut Checklist](./references/cross-cut-checklist.md):

1. **Logging & Error Handling**
   - [Logging Pattern](./references/logging-pattern.md): Use ILogger<T>, structured logging, log levels
   - Exception handling: try-catch with logging, no bare throws
   - All async operations logged (start, success, error)

2. **Dependency Injection**
   - All services registered in Program.cs (`AddScoped`, `AddSingleton`, `AddTransient`)
   - Logger injected: `ILogger<ClassName>` pattern
   - SqlConnection/IConnectionMultiplexer (Redis) correctly registered

3. **Database & Persistence**
   - EF Core migrations applied: `db.Database.Migrate()` on startup
   - Connection strings: `ConnectionStrings__ServiceName` (double underscore for environment)
   - Async patterns: `async Task`, `await`, no `.Result` or `.Wait()`

4. **Security & CORS**
   - CORS policy "AllowAll" documented (dev-only consideration)
   - Kestrel endpoint configuration: `ListenAnyIP(port)`
   - No hardcoded credentials in code

5. **Configuration**
   - `appsettings.json` and `appsettings.Development.json` separation
   - Environment variables: `ASPNETCORE_ENVIRONMENT`, connection strings
   - Sensitive data: never in source control

### Phase 4: Code Quality

Review [Code Quality Checklist](./references/code-quality-checklist.md):

1. **Naming & Style**
   - PascalCase for public members, camelCase for private/parameters
   - Meaningful names: `GetBasketAsync`, not `GetB()`
   - Method names reflect async: `GetBasketAsync`, not `GetBasket`

2. **Error Messages & Logging**
   - Structured logging with parameters: `_logger.LogInformation("Basket updated: {BuyerId}", buyerId)`
   - Error messages include context: user/resource/action
   - Exception types specific (ArgumentNullException, InvalidOperationException, not generic Exception)

3. **Performance & Async**
   - All I/O operations async (Redis, EF Core, HTTP calls)
   - No blocking: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`
   - Connection pooling configured (IConnectionMultiplexer singleton pattern)

4. **Testing Readiness**
   - Dependencies injectable (interfaces, not concrete types)
   - Pure functions (no static dependencies making testing hard)
   - Separation of concerns enabled unit testing

### Phase 5: Create Review Summary

1. Run through applicable checklist items above
2. Document findings by category: ✅ Passed | ⚠️ Minor Issue | ❌ Blocking Issue
3. Reference specific files and line numbers
4. Provide actionable feedback with examples
5. Use [review template](./templates/review-template.md) format for consistency

## Common Review Patterns

### Adding a New Feature

1. Check domain entity has validation logic (constructor, private setters)
2. Verify application service exists with interface
3. Confirm DTOs separate API contract from domain
4. Validate controller endpoint structure
5. Check infrastructure repository pattern
6. Verify logging added at key decision points

### Database Migrations

1. Migration file named meaningfully: `AddProductInventory`
2. Implements `Up()` for forward, `Down()` for rollback
3. `db.Database.Migrate()` in Program.cs applied on startup
4. Connection string available in Environment/appsettings
5. No hardcoded data assumptions

### Redis/Caching Changes

1. IConnectionMultiplexer registered as singleton in DI
2. Logging added for cache hit/miss
3. Serialization/deserialization logged
4. TTL/expiration policies documented
5. Error handling for connection failures

### Logging & Error Handling

1. All async operations wrapped in try-catch
2. Exceptions logged with context
3. Structured logging with named parameters
4. Log levels appropriate: Info for business events, Debug for flow, Warning for issues
5. Sensitive data not logged

## Quick Reference

**File Locations by Service**
```
CatalogService/
├── Domain/Entities/Product.cs → [Entity + validation]
├── Application/Products/ → [DTOs, services, validators]
├── Infrastructure/Data/ → [DbContext, repositories]
└── API/Controllers/ → [Endpoints]

OrderService/
├── Domain/Entities/ → [Order, OrderItem]
├── Application/DTOs/ → [OrderCreateRequest, OrderResponse]
├── Infrastructure/Data/ → [OrderDbContext, repositories]
└── API/Controller/ → [OrderController]

BasketService/
├── Domain/Entities/Basket.cs
├── Application/Interfaces/ → [IBasketRepository]
├── Infrastructure/Data/ → [BasketRepository + Redis]
└── API/Controllers/ → [BasketController]
```

**Common Issues Found & Fixes**
| Issue | Pattern | Fix |
|-------|---------|-----|
| Missing logging | Event happens silently | Add `_logger.LogInformation()` with structured params |
| Anemic domain | Validation in service layer | Move to entity constructor + private setters |
| DTO leak | Domain entities in API response | Create separate DTO classes |
| No error handling | Unhandled exceptions crash | Wrap in try-catch with logging |
| Bare async | async Task/await without try-catch | Add exception handling for async ops |
| Connection issues | Redis/EF Core failures crash silently | Add error logging to connection setup |

## References

- [Clean Architecture Checklist](./references/clean-architecture-checklist.md)
- [Cross-Cut Concerns Checklist](./references/cross-cut-checklist.md)
- [Code Quality Checklist](./references/code-quality-checklist.md)
- [Logging Pattern Guide](./references/logging-pattern.md)
- [Review Template](./templates/review-template.md)
- [Issue Comment Template](./templates/issue-comment-template.md)

## Example: Reviewing Logging Addition

**Scenario**: PR adds logging to BasketService

**Checklist Items**:
1. ✅ Logging added to all public methods (GetBasket, UpdateBasket, DeleteBasket)
2. ✅ Structured parameters used: `{BuyerId}`, `{ItemCount}`
3. ✅ Log levels correct: Info for operations, Debug for details, Error for exceptions
4. ✅ Exception handling includes logging context
5. ✅ ILogger<T> injected via DI in constructors
6. ✅ Serilog configured in Program.cs with console sink
7. ✅ No sensitive data logged (passwords, API keys)
8. ✅ Async operations properly awaited

**Result**: Ready to merge ✅

---

*Last Updated: April 2026 | For .NET 9 Microservices Solution*
