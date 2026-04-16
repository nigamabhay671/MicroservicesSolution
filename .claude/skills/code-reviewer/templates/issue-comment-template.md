# Issue Comment Template

Use this template for commenting on specific issues within a pull request.

## Comment Types

### ✅ Approval Comment

```
Great work on the logging implementation! This follows the established patterns well.

**What I liked:**
- Structured logging with named parameters (e.g., `{BuyerId}`)
- Appropriate use of log levels (Information for operations, Debug for details)
- Exception handling includes logging context
- Clean separation of concerns in the repository

**Ready to merge!** ✅
```

### 🟡 Suggestion Comment

```
**Consider this improvement:**

The logging in `GetBasketAsync` could include the item count for better visibility into basket size:

```csharp
_logger.LogInformation("Updating basket: {BuyerId} with {ItemCount} items", 
    basket.BuyerId, basket.Items?.Count ?? 0);
```

This helps with performance analysis and usage patterns. Not required, just a suggestion!
```

### 🔴 Blocking Issue Comment

```
**This needs to be fixed before merging:**

The `UpdateBasketAsync` method doesn't validate the basket before updating:

```csharp
public async Task<Basket> UpdateBasketAsync(Basket basket)
{
    // ❌ No null check or validation
    var created = await _database.StringSetAsync(...);
}
```

**Please add:**
```csharp
public async Task<Basket> UpdateBasketAsync(Basket basket)
{
    if (basket == null)
        throw new ArgumentNullException(nameof(basket));
    if (string.IsNullOrWhiteSpace(basket.BuyerId))
        throw new ArgumentException("BuyerId required", nameof(basket));
        
    var created = await _database.StringSetAsync(...);
}
```

**Why:** Prevents invalid state and improves error messages.
```

### ❌ Design Issue Comment

```
**Architecture concern:**

The `BasketRepository` is tightly coupled to Redis serialization. Consider:

**Current approach (tightly coupled):**
```csharp
public class BasketRepository : IBasketRepository
{
    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(buyerId);
        return JsonConvert.DeserializeObject<Basket>(data.ToString());
    }
}
```

**Better approach (uses abstraction):**
```csharp
public interface IBasketSerializer
{
    string Serialize(Basket basket);
    Basket Deserialize(string data);
}

public class BasketRepository : IBasketRepository
{
    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(buyerId);
        return _serializer.Deserialize(data.ToString());
    }
}
```

**Why:** Allows changing serialization format (JSON → Protobuf, etc.) without touching repository logic.

**Not blocking** if you prefer to keep it simple for now, but worth considering as the service grows.
```

### 📚 Educational Comment

```
**Learning note - Not required to change:**

I noticed you're using `await _database.StringSetAsync(...)` directly. This is correct! In case you're curious:

- `StringSetAsync` → Fire-and-forget operation
- Returns `Task<bool>` (success/failure)
- If we needed to wait for replication: use `IServer.InfoAsync()`
- When performance is critical, batch operations with `IBatch` interface

This is already correctly implemented! Just context if you want to explore further. 📖
```

### 🤔 Question Comment

```
**Question for clarification:**

In the `DeleteBasketAsync` method, why log both warning and return false separately?

```csharp
if (!result)
{
    _logger.LogWarning("Basket not found for deletion, buyerId: {BuyerId}", buyerId);
}
return result;
```

Could this be simplified to:
```csharp
if (!result)
    _logger.LogWarning("Basket not found for deletion, buyerId: {BuyerId}", buyerId);
return result;
```

Not a blocker—just wondering about style consistency with other methods.
```

## Comment Templates

### Template: Praise
```
✨ **Great catch!** - [specific praise]
[Reasoning]
```

### Template: Bug Report
```
🐛 **Found a bug:**
- **Issue**: [Description]
- **File**: [Link]
- **Fix**: [Suggested fix]
- **Impact**: [Why it matters]
```

### Template: Style Issue
```
📝 **Style suggestion:**
- **Current**: [Code snippet]
- **Suggested**: [Code snippet]
- **Reason**: [Explanation]
[Optional: "Not required, just for consistency"]
```

### Template: Performance Note
```
⚡ **Performance note:**
- **Current behavior**: [Description]
- **Potential issue**: [Explanation]
- **Suggestion**: [Alternative approach]
[Optional: "Benchmark first before changing"]
```

### Template: Security Note
```
🔒 **Security review:**
- **Current**: [Code snippet]
- **Concern**: [Explanation]
- **Recommendation**: [Fix or verification needed]
```

## Guidelines

**Be Constructive**
- Frame as observations, not judgments
- Explain the "why"
- Offer concrete suggestions
- Acknowledge trade-offs

**Be Specific**
- Reference file names and line numbers
- Provide example code
- Show before/after
- Link to relevant docs/standards

**Be Kind**
- Praise good work
- Assume good intent
- Focus on the code, not the person
- Offer help or pair programming if complex

**Know What's Blocking**
- ❌ Blocking issues must be fixed (security, architecture violations, bugs)
- 🟡 Minor issues should be addressed but not blockers
- 🟢 Nice-to-haves are optional (style, performance optimization, educational)

---

*See CODE_REVIEW_CHECKLISTS.md for structured assessment framework*
