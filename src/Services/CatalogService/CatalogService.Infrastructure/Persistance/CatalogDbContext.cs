using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure;

public class CatalogDbContext:DbContext
{
     public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Sku).IsRequired().HasMaxLength(64);
            e.Property(x => x.Name).IsRequired().HasMaxLength(256);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.Sku).IsUnique();
        });
    }

}
