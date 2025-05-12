namespace EShopDomain.Models;

public class Product : BaseModel
{
    public string Ean { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; } = 0;

    public string Sku { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}