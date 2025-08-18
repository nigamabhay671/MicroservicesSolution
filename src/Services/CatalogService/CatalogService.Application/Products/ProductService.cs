using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CatalogService.Domain;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Repositories;

namespace CatalogService.Application.Products
{
    public interface IProductAppService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default);
        Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ProductDto> CreateAsync(CreateProductRequest req, CancellationToken ct = default);
    }

    public class ProductAppService : IProductAppService
    {
        private readonly IProductRepository _repo;
        private readonly IMapper _mapper;

        public ProductAppService(IProductRepository repo, IMapper mapper)
        {
            _repo = repo; _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default)
            => (await _repo.GetAllAsync(ct)).Select(_mapper.Map<ProductDto>);

        public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
            => (await _repo.GetByIdAsync(id, ct)) is { } p ? _mapper.Map<ProductDto>(p) : null;

        public async Task<ProductDto> CreateAsync(CreateProductRequest req, CancellationToken ct = default)
        {
            var entity = new Product(req.Sku, req.Name, req.Price, req.Stock, req.Description);
            await _repo.AddAsync(entity, ct);
            return _mapper.Map<ProductDto>(entity);
        }
    }
}