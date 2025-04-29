using ActivityLogger.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ProductCatalog.Client;
using ShoppingCard.Client;

namespace BlazorUI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new ProductCatalogClient(new HttpClient { BaseAddress = new Uri("http://localhost:5001") }));
            builder.Services.AddScoped(sp => new ShoppingCardClient(new HttpClient { BaseAddress = new Uri("http://localhost:5002") }));
            builder.Services.AddScoped(sp => new ActivityLoggerClient(new HttpClient { BaseAddress = new Uri("http://localhost:5003") }));
            builder.Services.AddScoped<DataService>();

            await builder.Build().RunAsync();
        }
    }
}
