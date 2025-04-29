//  Generated from ProductCatalog.Service.Controllers.ProductCatalogController Don't change it!!!
using System.Net.Http.Json;
using MicroCommerce.Models;

namespace ProductCatalog.Client
{
    public class ProductCatalogClient
    {
        private readonly HttpClient _httpClient;

        public ProductCatalogClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Product>> GetList()
        {
            var response = await _httpClient.GetAsync($"");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<Product>>();
        }

        public async Task<Product> GetProduct(Guid productId)
        {
            var response = await _httpClient.GetAsync($"{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

    }
}
