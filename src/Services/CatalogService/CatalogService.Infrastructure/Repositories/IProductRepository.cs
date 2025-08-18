using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogService.Domain.Entities;

namespace CatalogService.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
        Task<List<Product>> GetAllAsync(CancellationToken ct = default);
        Task<Product> AddAsync(Product p, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
