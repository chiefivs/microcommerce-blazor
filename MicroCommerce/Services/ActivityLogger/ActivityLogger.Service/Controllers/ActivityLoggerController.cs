using MicroCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using ShoppingCard.Client;
using System.Diagnostics;

namespace ActivityLogger.Service.Controllers
{
    [ApiController]
    [Route("")]
    public class ActivityLoggerController : ControllerBase
    {
        private ShoppingCardClient _shoppingCard;
        private ILogger<ActivityLoggerController> _logger;

        private static long timestamp;
        private static List<LogEvent> _log = new List<LogEvent>();

        public ActivityLoggerController(ShoppingCardClient shoppingCard, ILogger<ActivityLoggerController> logger)
        {
            _shoppingCard = shoppingCard;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IEnumerable<LogEvent>> Get(long timestamp)
        {
            Activity.Current?.AddTag("timestamp", timestamp);
            _logger.LogWarning($"Get events {timestamp}");

            await ReceiveEvents();
            return _log.Where(i => i.Timestamp > timestamp);
        }

        [HttpDelete("clear")]
        public async Task<IEnumerable<LogEvent>> Clear()
        {
            _logger.LogWarning("Clear");

            lock (_log)
            {
                _log.Clear();
                return _log;
            }
        }

        private async Task ReceiveEvents()
        {
            _logger.LogWarning($"ReceiveEvents {timestamp}");
            var cardEvents = await _shoppingCard.GetCardEvents(timestamp);

            if (cardEvents.Count() > 0)
            {
                timestamp = cardEvents.Max(c => c.Timestamp);
                lock (_log)
                {
                    _log.AddRange(cardEvents.Select(e => new LogEvent
                    {
                        Description = $"{GetEventDesc(e.Type)}: '{e.Order.Product.Name} ({e.Order.Quantity})'"
                    }));
                }
            }
        }

        private string GetEventDesc(CardEventTypeEnum type)
        {
            using var activity = Activity.Current?.Source.StartActivity();
            activity?.AddTag("type", type);

            switch (type)
            {
                case CardEventTypeEnum.OrderAdded: return "order added";
                case CardEventTypeEnum.OrderChanged: return "order changed";
                case CardEventTypeEnum.OrderRemoved: return "order removed";
                default: return "unknown operation";
            }
        }
    }
}
