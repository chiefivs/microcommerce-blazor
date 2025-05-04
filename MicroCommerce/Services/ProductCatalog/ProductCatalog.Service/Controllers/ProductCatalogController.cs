using MicroCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ProductCatalog.Service.Controllers
{
    [ApiController]
    [Route("")]
    public class ProductCatalogController : ControllerBase
    {
        private readonly ILogger<ProductCatalogController> _logger;

        public ProductCatalogController(ILogger<ProductCatalogController> logger)
        {
            _logger = logger;
        }

        private Product[] _products = new[]
        {
            new Product{ Id = new Guid("6BF3A1CE-1239-4528-8924-A56FF6527595"), Name = "T-shirt" },
            new Product{ Id = new Guid("6BF3A1CE-1239-4528-8924-A56FF6527596"), Name = "Hoodie" },
            new Product{ Id = new Guid("6BF3A1CE-1239-4528-8924-A56FF6527597"), Name = "Trousers" }
        };

        [HttpGet]
        public async Task<IEnumerable<Product>> GetList()
        {
            _logger.LogWarning("Get all products");
            return _products;
        }

        [HttpGet("{productId}")]
        public async Task<Product> GetProduct(Guid productId)
        {
            Activity.Current?.AddTag("productId", productId);
            _logger.LogWarning($"Get product {productId}");

            return _products.FirstOrDefault(p => p.Id == productId);
        }

        //[HttpDelete("{productId}/delete")]
        //public async Task DeleteProduct(Guid productId)
        //{

        //}

        //[HttpDelete("{productId}/remove")]
        //public async Task<bool> RemoveProduct(Guid productId)
        //{
        //    return true;
        //}

        //[HttpPost("add")]
        //public async Task<ActionResult> AddProduct([FromBody] Product product, [FromQuery] bool active)
        //{
        //    return Ok();
        //}

        //[HttpPost("create")]
        //public async Task<ActionResult<Product?>> CreateProduct([FromBody] Product product)
        //{
        //    return product;
        //}

        //[HttpPut("update/{id}")]
        //public async Task UpdateProduct(Guid id, [FromBody] Product product)
        //{

        //}

        //[HttpPatch("modify/{id}")]
        //public async Task<Product> ModifyProduct(Guid id, [FromBody] Product product)
        //{
        //    return product;
        //}
    }
}
