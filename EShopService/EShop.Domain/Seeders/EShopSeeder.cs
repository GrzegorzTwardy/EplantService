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
                    new Category { Name = "Klocki" },
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
            if (!context.Products.Any())
            {
                var category = await context.Categories
                        .Where(x => x.Name == "Klocki").FirstOrDefaultAsync();

                var products = new List<Product>
                {
                    new Product { Name = "Cobi", Ean = "1234", Category = category },
                    new Product { Name = "Duplo", Ean = "431", Category = category },
                    new Product { Name = "Lego", Ean = "12212", Category = category }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
