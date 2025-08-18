using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _db;
        public ProductRepository(CatalogDbContext db) => _db = db;

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
            => _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
            => _db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);

        public Task<List<Product>> GetAllAsync(CancellationToken ct = default)
            => _db.Products.OrderBy(p => p.Id).ToListAsync(ct);

        public async Task<Product> AddAsync(Product p, CancellationToken ct = default)
        {
            _db.Products.Add(p);
            await _db.SaveChangesAsync(ct);
            return p;
        }

        public Task SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
