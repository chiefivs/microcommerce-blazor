using ActivityLogger.Client;
using MicroCommerce.Models;
using ProductCatalog.Client;
using ShoppingCard.Client;

namespace BlazorUI
{
    public class DataService
    {
        private readonly ProductCatalogClient _productCatalog;
        private readonly ShoppingCardClient _shoppingCard;
        private readonly ActivityLoggerClient _activityLogger;

        private IEnumerable<Product> _products = null;
        private IEnumerable<Order> _orders = null;
        private List<LogEvent> _logEvents = new List<LogEvent>();

        public event Action? OnProductsChanged;
        public IEnumerable<Product> Products
        {
            get { return _products; }
            set
            {
                _products = value;
                OnProductsChanged?.Invoke();
            }
        }

        public event Action? OnOrdersChanged;
        public IEnumerable<Order> Orders
        {
            get { return _orders; }
            set
            {
                _orders = value;
                OnOrdersChanged?.Invoke();
            }
        }

        public event Action? OnLogEventssChanged;
        public IEnumerable<LogEvent> LogEvents
        {
            get { return _logEvents; }
        }

        private long _timestamp = 0;
        private bool _updateLogsInProgress = false;

        public DataService(ProductCatalogClient productCatalog, ShoppingCardClient shoppingCard, ActivityLoggerClient activityLogger)
        {
            _productCatalog = productCatalog;
            _shoppingCard = shoppingCard;
            _activityLogger = activityLogger;
        }

        public async Task LoadProducts()
        {
            try
            {
            Products = await _productCatalog.GetList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task LoadCard()
        {
            try
            {
                var card = await _shoppingCard.Get();
                Orders = card.Orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task AddOrder(Guid productId, int qty)
        {
            try
            {
                var card = await _shoppingCard.AddOrder(productId, qty);
                Orders = card.Orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                UpdateLogsWithTimeout();
            }
        }

        public async Task DelOrder(Guid orderId)
        {
            try
            {
                var card = await _shoppingCard.DeleteOrder(orderId);
                Orders = card.Orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                UpdateLogsWithTimeout();
            }
        }

        public async Task ClearLogs()
        {
            try
            {
                await _activityLogger.Clear();
                _timestamp = 0;
                _logEvents.Clear();
                OnLogEventssChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task UpdateLogs()
        {
            if (_updateLogsInProgress)
                return;

            try
            {
                _updateLogsInProgress = true;
                var _events = await _activityLogger.Get(_timestamp);
                foreach (var evt in _events)
                {
                    _logEvents.Add(evt);
                    _timestamp = Math.Max(_timestamp, evt.Timestamp);
                }
                OnLogEventssChanged?.Invoke();

                Console.WriteLine("update logs: {0}", _timestamp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _updateLogsInProgress = false;
            }
        }

        private async Task UpdateLogsWithTimeout()
        {
            await Task.Delay(2000);
            await UpdateLogs();
        }
    }
}
