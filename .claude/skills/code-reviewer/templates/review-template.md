# Code Review Template

Use this template format when conducting code reviews for structured, consistent feedback.

## Review Header

```
## Code Review: [Feature/PR Title]
**Reviewer**: [Name]
**Date**: [Date]
**Files Changed**: [Number]
**Status**: [✅ Approved | ⚠️ Changes Requested | ❌ Blocked]

**Summary**: [1-2 sentence overview of changes]
```

## Section: Clean Architecture

### Domain Layer
- [✅/⚠️/❌] **Issue**: [Description]
  - **File**: [`Path/To/File.cs`](Path/To/File.cs#L10)
  - **Details**: [Specific feedback]
  - **Suggestion**: [How to fix]

**Example**:
```
- ⚠️ **Entity validation**: Product constructor should validate Price parameter
  - **File**: [`src/Services/CatalogService/CatalogService.Domain/Entities/Product.cs`](file.cs#L15)
  - **Details**: Price is accepted without checking if it's negative
  - **Suggestion**: Add `if (price < 0) throw new ArgumentException(...)` in constructor
```

### Application Layer
- [✅/⚠️/❌] **Issue**: [Description]
  - **Files**: [`DTOs/`](DTOs/), [`Services/`](Services/), [`Validators/`](Validators/)
  - **Details**: [Specific feedback]

### Infrastructure Layer
- [✅/⚠️/❌] **Issue**: [Description]
  - **Files**: [`Persistance/`](Persistance/), [`Data/`](Data/), [`Migrations/`](Migrations/)
  - **Details**: [Specific feedback]

### API Layer
- [✅/⚠️/❌] **Issue**: [Description]
  - **Files**: [`Controllers/`](Controllers/), [`Program.cs`](Program.cs)
  - **Details**: [Specific feedback]

## Section: Cross-Cutting Concerns

### Logging & Error Handling
- [✅/⚠️/❌] **Completeness**: Logging added to all public methods
  - **Status**: ✅ All public methods have entry/exit logging
  - **Details**: GetBasket, UpdateBasket, DeleteBasket all include structured logging

- [✅/⚠️/❌] **Levels**: Appropriate log levels used
  - **Status**: ✅ Information for operations, Debug for details, Error for exceptions
  - **Example**: `_logger.LogInformation("Updating basket: {BuyerId}", buyerId)`

### Dependency Injection
- [✅/⚠️/❌] **Registration**: Services registered correctly in Program.cs
  - **Status**: ✅ IConnectionMultiplexer registered as singleton
  - **Status**: ✅ IBasketRepository registered as scoped

### Database & Persistence
- [✅/⚠️/❌] **Async Operations**: No blocking calls detected
  - **Status**: ✅ All I/O operations use async/await

### Security & Configuration
- [✅/⚠️/❌] **Secrets**: No hardcoded credentials
  - **Status**: ✅ Connection strings from configuration

## Section: Code Quality

### Naming Conventions
- [✅/⚠️/❌] **Methods**: Async methods end with "Async"
  - **Status**: ✅ GetBasketAsync, UpdateBasketAsync, DeleteBasketAsync
- [✅/⚠️/❌] **Fields**: Private fields use camelCase with underscore
  - **Status**: ✅ `_repository`, `_logger`

### Error Messages
- [✅/⚠️/❌] **Clarity**: Error messages are clear and actionable
  - **Status**: ✅ Exception types specific, messages include context

### Readability
- [✅/⚠️/❌] **Method Length**: Methods reasonably sized (<30 lines)
  - **Status**: ✅ Controllers and repositories follow guidelines

## Issues Detailed

### 🔴 Blocking Issues (Must Fix)
None identified.

### 🟡 Minor Issues (Should Fix)
None identified.

### 🟢 Nice-to-Haves (Consider)
None identified.

## Testing Readiness

- [✅] Dependencies injectable (interfaces, not concrete)
- [✅] Repositories mockable
- [Comment]: Consider adding unit tests for basket service

## Summary

**Strengths**:
- Well-structured Clean Architecture compliance
- Comprehensive logging throughout
- Proper error handling and exception logging
- Correct use of async/await patterns

**Areas for Improvement**:
- None found in this review

**Recommendation**: ✅ **APPROVED - Ready to Merge**

---

**Checklist for Approval**:
- [✅] Architecture compliance verified
- [✅] Logging and error handling complete
- [✅] Security and configuration reviewed
- [✅] Code quality meets standards
- [✅] Async/await patterns correct
- [✅] Testing readiness confirmed
