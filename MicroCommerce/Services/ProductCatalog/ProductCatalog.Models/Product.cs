namespace ProductCatalog.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Product Clone()
        {
            return new Product
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
