using EShop.Domain.Repositories;
using EShopDomain.Models;
using Microsoft.EntityFrameworkCore;

namespace EShop.Domain.Seeders
{
    public class EShopSeeder(DataContext context) : IEShopSeeder
    {
        public async Task Seed()
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Rosliny" },
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
            if (!context.Products.Any())
            {
                var category = await context.Categories
                        .Where(x => x.Name == "Rosliny").FirstOrDefaultAsync();

                var products = new List<Product>
                {
                    new Product { Name = "Kaktus", Sku = "8559921040708", Category = category },
                    new Product { Name = "Bratek", Sku = "5903837350117", Category = category },
                    new Product { Name = "Chryzantema", Sku = "5907671252200", Category = category }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
