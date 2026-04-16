using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace BasketService.Infrastructure.Data;

public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<BasketRepository> _logger;

    public BasketRepository(IConnectionMultiplexer redis, ILogger<BasketRepository> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        _logger.LogDebug("Fetching basket from Redis for buyerId: {BuyerId}", buyerId);
        try
        {
            var data = await _database.StringGetAsync(buyerId);

            if (data.IsNullOrEmpty)
            {
                _logger.LogDebug("Basket not found in Redis for buyerId: {BuyerId}", buyerId);
                return null;
            }

            var basket = JsonConvert.DeserializeObject<Basket>(data.ToString());
            _logger.LogDebug("Successfully deserialized basket for buyerId: {BuyerId}", buyerId);
            return basket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket from Redis for buyerId: {BuyerId}", buyerId);
            throw;
        }
    }

    public async Task<Basket> UpdateBasketAsync(Basket basket)
    {
        _logger.LogDebug("Updating basket in Redis for buyerId: {BuyerId} with {ItemCount} items",
            basket.BuyerId, basket.Items?.Count ?? 0);
        try
        {
            var serialized = JsonConvert.SerializeObject(basket);
            var created = await _database.StringSetAsync(
                basket.BuyerId,
                serialized,
                TimeSpan.FromDays(30)); // Expiry 30 days

            if (!created)
            {
                _logger.LogError("Failed to set basket in Redis for buyerId: {BuyerId}", basket.BuyerId);
                return null;
            }

            _logger.LogInformation("Successfully updated basket in Redis for buyerId: {BuyerId}", basket.BuyerId);
            return await GetBasketAsync(basket.BuyerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating basket in Redis for buyerId: {BuyerId}", basket.BuyerId);
            throw;
        }
    }

    public async Task<bool> DeleteBasketAsync(string buyerId)
    {
        _logger.LogDebug("Deleting basket from Redis for buyerId: {BuyerId}", buyerId);
        try
        {
            var result = await _database.KeyDeleteAsync(buyerId);
            if (result)
            {
                _logger.LogInformation("Successfully deleted basket from Redis for buyerId: {BuyerId}", buyerId);
            }
            else
            {
                _logger.LogWarning("Basket not found in Redis for deletion, buyerId: {BuyerId}", buyerId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket from Redis for buyerId: {BuyerId}", buyerId);
            throw;
        }
    }
}
