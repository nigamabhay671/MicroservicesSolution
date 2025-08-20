using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrderService.API.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Repositories;

namespace OrderService.Application.OrderService
{
    public class OrderServiceApp : IOrderService
    {
        private readonly OrderRepository _repository;

        public OrderServiceApp(OrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<Order> CreateOrderAsync(OrderDto orderDto)
        {
            var order = new Order
            {
                BuyerId = orderDto.BuyerId,
                TotalAmount = orderDto.TotalAmount,
                OrderItems = orderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            await _repository.AddOrderAsync(order);
            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int id) =>
            await _repository.GetOrderByIdAsync(id);

        public async Task<List<Order>> GetAllOrdersAsync() =>
            await _repository.GetAllOrdersAsync();
    }
}
