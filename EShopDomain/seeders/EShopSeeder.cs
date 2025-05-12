using EShopDomain.Models;
using EShopDomain.Repositories;

namespace EShopDomain.seeders;

public interface IEShopSeeder
{
    void Seed();
}

public class EShopSeeder : IEShopSeeder
{
    private readonly DataContext _context;

    public EShopSeeder(DataContext context)
    {
        _context = context;
    }

    public void Seed()
    {
        // Seed categories if they don't exist
        if (!_context.Categories.Any()) SeedCategories();

        // Seed products if they don't exist
        if (!_context.Products.Any()) SeedProducts();
    }

    private void SeedCategories()
    {
        var categories = new List<Category>
        {
            new() { Name = "Electronics", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() },
            new() { Name = "Clothing", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() },
            new() { Name = "Books", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() }
        };

        _context.Categories.AddRange(categories);
        _context.SaveChanges();
    }

    private void SeedProducts()
    {
        // Get the seeded categories
        var categories = _context.Categories.ToList();

        var products = new List<Product>
        {
            new()
            {
                Name = "Smartphone X1",
                Ean = "5901234123457",
                Price = 999.99m,
                Stock = 50,
                Sku = "SMRTX1",
                CategoryId = categories[0].Id,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new()
            {
                Name = "T-Shirt",
                Ean = "5901234123458",
                Price = 29.99m,
                Stock = 100,
                Sku = "TSHRT1",
                CategoryId = categories[1].Id,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            },
            new()
            {
                Name = "Programming Book",
                Ean = "5901234123459",
                Price = 49.99m,
                Stock = 30,
                Sku = "BOOK1",
                CategoryId = categories[2].Id,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }
}