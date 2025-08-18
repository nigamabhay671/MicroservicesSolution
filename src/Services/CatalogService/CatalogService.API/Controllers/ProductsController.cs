using CatalogService.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductAppService _svc;
        public ProductsController(IProductAppService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Get(CancellationToken ct)
            => Ok(await _svc.GetAllAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
            => (await _svc.GetByIdAsync(id, ct)) is { } dto ? Ok(dto) : NotFound();

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductRequest req, CancellationToken ct)
        {
            var dto = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
    }
}
