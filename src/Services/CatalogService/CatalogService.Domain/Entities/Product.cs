namespace CatalogService.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Sku { get; private set; } = "";
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedUtc { get; private set; } = DateTime.UtcNow;

    private Product() { }
    public Product(string sku, string name, decimal price, int stock, string? desc = null)
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required");
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (stock < 0) throw new ArgumentOutOfRangeException(nameof(stock));

        Sku = sku.Trim();
        Name = name.Trim();
        Price = price;
        Stock = stock;
        Description = desc;
    }

    public void AdjustStock(int delta)
    {
        var newStock = Stock + delta;
        if (newStock < 0) throw new InvalidOperationException("Insufficient stock");
        Stock = newStock;
    }

}
