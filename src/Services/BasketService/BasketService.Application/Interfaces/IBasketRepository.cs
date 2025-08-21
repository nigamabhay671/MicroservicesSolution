using BasketService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketService.Application.Interfaces;

    public interface IBasketRepository
    {
        Task<Basket?> GetBasketAsync(string buyerId);
        Task<Basket?> UpdateBasketAsync(Basket basket);
        Task<bool> DeleteBasketAsync(string buyerId);
    }

