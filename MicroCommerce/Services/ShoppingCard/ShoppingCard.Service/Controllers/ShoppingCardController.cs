using MicroCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Client;
using System.Diagnostics;

namespace ShoppingCard.Service.Controllers
{
    [ApiController]
    [Route("")]
    public class ShoppingCardController : ControllerBase
    {
        private static List<Order> _orders = new List<Order>();
        private static List<CardEvent> _events = new List<CardEvent>();

        private readonly ProductCatalogClient _catalog;
        private readonly ILogger<ShoppingCardController> _logger;

        public ShoppingCardController(ProductCatalogClient catalog, ILogger<ShoppingCardController> logger)
        {
            _catalog = catalog;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<Card> Get()
        {
            _logger.LogWarning("Get cart");

            return new Card
            {
                Orders = _orders
            };
        }

        [HttpPut("addorder/{productId}/{qty}")]
        public async Task<Card> AddOrder(Guid productId, int qty)
        {
            Activity.Current?.AddTag("productId", productId);
            Activity.Current?.AddTag("qty", qty);
            _logger.LogWarning($"AddOrder {productId} {qty}");

            var order = _orders.FirstOrDefault(i => i.Product.Id == productId);
            if (order != null)
            {
                order.Quantity += qty;
                CreateEvent(CardEventTypeEnum.OrderChanged, order);
            }
            else
            {
                var product = await _catalog.GetProduct(productId);
                if (product != null)
                {
                    using (var activity = Activity.Current?.Source.StartActivity("Create a new order position"))
                    {
                        order = new Order
                        {
                            Id = Guid.NewGuid(),
                            Product = product,
                            Quantity = qty
                        };

                        activity?.AddTag("orderId", order.Id);
                        _orders.Add(order);
                    }
                    CreateEvent(CardEventTypeEnum.OrderAdded, order);
                }
            }

            return await Get();
        }

        [HttpDelete("delorder/{orderId}")]
        public async Task<Card> DeleteOrder(Guid orderId)
        {
            Activity.Current?.AddTag("orderId", orderId);
            _logger.LogWarning($"DeleteOrder {orderId}");

            var order = _orders.FirstOrDefault(i => i.Id == orderId);
            if (order != null)
            {
                _orders.Remove(order);
                CreateEvent(CardEventTypeEnum.OrderRemoved, order);
            }

            return await Get();
        }

        [HttpGet("getevents/{timestamp}")]
        public async Task<IEnumerable<CardEvent>> GetCardEvents(long timestamp)
        {
            Activity.Current?.AddTag("timestamp", timestamp);
            _logger.LogWarning($"GetCartEvents {timestamp}");

            return _events.Where(e => e.Timestamp > timestamp);
        }

        private void CreateEvent(CardEventTypeEnum type, Order order)
        {
            using var activity = Activity.Current?.Source.StartActivity();
            activity?.AddTag("type", type);
            activity?.AddTag("order", order);
            _events.Add(new CardEvent
            {
                Timestamp = DateTime.Now.Ticks,
                Time = DateTime.Now,
                Order = order.Clone(),
                Type = type
            });
        }
    }
}
