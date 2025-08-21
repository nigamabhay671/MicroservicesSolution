using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace BasketService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;

    public BasketController(IBasketRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{buyerId}")]
    public async Task<ActionResult<Basket>> GetBasket(string buyerId)
    {
        var basket = await _repository.GetBasketAsync(buyerId);
        if (basket == null)
            return NotFound();
        return Ok(basket);
    }

    [HttpPost]
    public async Task<ActionResult<Basket>> UpdateBasket(Basket basket)
    {
        var updated = await _repository.UpdateBasketAsync(basket);
        return Ok(updated);
    }

    [HttpDelete("{buyerId}")]
    public async Task<IActionResult> DeleteBasket(string buyerId)
    {
        var deleted = await _repository.DeleteBasketAsync(buyerId);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
