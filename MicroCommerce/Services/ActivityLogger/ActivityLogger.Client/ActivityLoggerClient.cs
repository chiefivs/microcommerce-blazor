//  Generated from ActivityLogger.Service.Controllers.ActivityLoggerController Don't change it!!!
using System.Net.Http.Json;
using MicroCommerce.Models;

namespace ActivityLogger.Client
{
    public class ActivityLoggerClient
    {
        private readonly HttpClient _httpClient;

        public ActivityLoggerClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LogEvent>> Get(long timestamp)
        {
            var response = await _httpClient.GetAsync($"?timestamp={timestamp}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<LogEvent>>();
        }

        public async Task<IEnumerable<LogEvent>> Clear()
        {
            var response = await _httpClient.DeleteAsync($"clear");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<LogEvent>>();
        }

    }
}
