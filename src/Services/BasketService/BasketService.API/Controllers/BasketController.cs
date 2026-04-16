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
    private readonly ILogger<BasketController> _logger;

    public BasketController(IBasketRepository repository, ILogger<BasketController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("{buyerId}")]
    public async Task<ActionResult<Basket>> GetBasket(string buyerId)
    {
        _logger.LogInformation("Getting basket for buyerId: {BuyerId}", buyerId);
        try
        {
            var basket = await _repository.GetBasketAsync(buyerId);
            if (basket == null)
            {
                _logger.LogWarning("Basket not found for buyerId: {BuyerId}", buyerId);
                return NotFound();
            }
            _logger.LogDebug("Successfully retrieved basket for buyerId: {BuyerId}", buyerId);
            return Ok(basket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket for buyerId: {BuyerId}", buyerId);
            throw;
        }
    }

    [HttpPost]
    public async Task<ActionResult<Basket>> UpdateBasket(Basket basket)
    {
        _logger.LogInformation("Updating basket for buyerId: {BuyerId} with {ItemCount} items",
            basket.BuyerId, basket.Items?.Count ?? 0);
        try
        {
            var updated = await _repository.UpdateBasketAsync(basket);
            if (updated == null)
            {
                _logger.LogError("Failed to update basket for buyerId: {BuyerId}", basket.BuyerId);
                return BadRequest("Failed to update basket");
            }
            _logger.LogDebug("Successfully updated basket for buyerId: {BuyerId}", basket.BuyerId);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket for buyerId: {BuyerId}", basket.BuyerId);
            throw;
        }
    }

    [HttpDelete("{buyerId}")]
    public async Task<IActionResult> DeleteBasket(string buyerId)
    {
        _logger.LogInformation("Deleting basket for buyerId: {BuyerId}", buyerId);
        try
        {
            var deleted = await _repository.DeleteBasketAsync(buyerId);
            if (!deleted)
            {
                _logger.LogWarning("Basket not found for deletion, buyerId: {BuyerId}", buyerId);
                return NotFound();
            }
            _logger.LogDebug("Successfully deleted basket for buyerId: {BuyerId}", buyerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for buyerId: {BuyerId}", buyerId);
            throw;
        }
    }
}
