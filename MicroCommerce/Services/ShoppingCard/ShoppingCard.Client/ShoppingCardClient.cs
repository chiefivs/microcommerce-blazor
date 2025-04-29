//  Generated from ShoppingCard.Service.Controllers.ShoppingCardController Don't change it!!!
using System.Net.Http.Json;
using MicroCommerce.Models;

namespace ShoppingCard.Client
{
    public class ShoppingCardClient
    {
        private readonly HttpClient _httpClient;

        public ShoppingCardClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Card> Get()
        {
            var response = await _httpClient.GetAsync($"");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Card>();
        }

        public async Task<Card> AddOrder(Guid productId, int qty)
        {
            var response = await _httpClient.PutAsJsonAsync($"addorder/{productId}/{qty}", new object());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Card>();
        }

        public async Task<Card> DeleteOrder(Guid orderId)
        {
            var response = await _httpClient.DeleteAsync($"delorder/{orderId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Card>();
        }

        public async Task<IEnumerable<CardEvent>> GetCardEvents(long timestamp)
        {
            var response = await _httpClient.GetAsync($"getevents/{timestamp}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<CardEvent>>();
        }

    }
}
