using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Application.Products
{
    public record ProductDto(int Id, string Sku, string Name, string? Description, decimal Price, int Stock);
    public record CreateProductRequest(string Sku, string Name, decimal Price, int Stock, string? Description);
}
