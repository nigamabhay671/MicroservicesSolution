using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace BasketService.Infrastructure.Data;
public class BasketRepository : IBasketRepository
{
    private readonly IDatabase _database;

    public BasketRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<Basket> GetBasketAsync(string buyerId)
    {
        var data = await _database.StringGetAsync(buyerId);

        if (data.IsNullOrEmpty)
            return null;

        return JsonConvert.DeserializeObject<Basket>(data.ToString());
    }

    public async Task<Basket> UpdateBasketAsync(Basket basket)
    {
        var created = await _database.StringSetAsync(
            basket.BuyerId,
            JsonConvert.SerializeObject(basket),
            TimeSpan.FromDays(30)); // Expiry 30 days

        if (!created)
            return null;

        return await GetBasketAsync(basket.BuyerId);
    }

    public async Task<bool> DeleteBasketAsync(string buyerId)
    {
        return await _database.KeyDeleteAsync(buyerId);
    }
}
