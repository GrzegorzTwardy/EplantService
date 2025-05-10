namespace EShopDomain.Models
{
    public class Plant : Product
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; } = 0;

        public string Sku { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public int EstimatedHeight { get; set; } = 0;

        public Category Category { get; set; } = default!;
    }
}
