using EShop.Domain.Models;
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
                    new Plant { Name = "Aloes", Sku = "5900000000001", Price = 19.99m, Stock = 20, Color = "Zielony", EstimatedHeight = 30, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Kaktus", Sku = "5900000000002", Price = 14.99m, Stock = 50, Color = "Zielony", EstimatedHeight = 15, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Bratek", Sku = "5900000000003", Price = 9.99m, Stock = 100, Color = "Fioletowy", EstimatedHeight = 10, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Chryzantema", Sku = "5900000000004", Price = 12.49m, Stock = 60, Color = "Żółty", EstimatedHeight = 35, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Juka", Sku = "5900000000005", Price = 39.99m, Stock = 10, Color = "Zielony", EstimatedHeight = 90, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Sansewieria", Sku = "5900000000006", Price = 24.99m, Stock = 30, Color = "Zielono-żółty", EstimatedHeight = 70, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Fikus", Sku = "5900000000007", Price = 29.99m, Stock = 15, Color = "Zielony", EstimatedHeight = 80, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Monstera", Sku = "5900000000008", Price = 44.99m, Stock = 25, Color = "Zielony", EstimatedHeight = 100, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Paprotka", Sku = "5900000000009", Price = 17.49m, Stock = 40, Color = "Zielony", EstimatedHeight = 40, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Lawenda", Sku = "5900000000010", Price = 13.99m, Stock = 35, Color = "Fioletowy", EstimatedHeight = 25, Category = PredefinedCategories.Outdoor },

                    new Plant { Name = "Bazylia", Sku = "5900000000011", Price = 7.49m, Stock = 100, Color = "Zielony", EstimatedHeight = 20, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Mięta", Sku = "5900000000012", Price = 7.99m, Stock = 90, Color = "Zielony", EstimatedHeight = 25, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Rozmaryn", Sku = "5900000000013", Price = 8.49m, Stock = 70, Color = "Zielony", EstimatedHeight = 30, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Trawa ozdobna", Sku = "5900000000014", Price = 11.99m, Stock = 50, Color = "Zielony", EstimatedHeight = 60, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Begonia", Sku = "5900000000015", Price = 10.99m, Stock = 40, Color = "Czerwony", EstimatedHeight = 20, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Pelargonia", Sku = "5900000000016", Price = 11.49m, Stock = 30, Color = "Różowy", EstimatedHeight = 30, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Hiacynt", Sku = "5900000000017", Price = 9.49m, Stock = 25, Color = "Niebieski", EstimatedHeight = 25, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Stokrotka", Sku = "5900000000018", Price = 6.99m, Stock = 80, Color = "Biały", EstimatedHeight = 15, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Petunia", Sku = "5900000000019", Price = 10.49m, Stock = 60, Color = "Fioletowy", EstimatedHeight = 20, Category = PredefinedCategories.Outdoor },

                    new Plant { Name = "Skrzydłokwiat", Sku = "5900000000020", Price = 23.99m, Stock = 20, Color = "Biały", EstimatedHeight = 50, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Kalanchoe", Sku = "5900000000021", Price = 12.99m, Stock = 35, Color = "Pomarańczowy", EstimatedHeight = 25, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Zamiokulkas", Sku = "5900000000022", Price = 27.49m, Stock = 18, Color = "Zielony", EstimatedHeight = 60, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Cytryniec", Sku = "5900000000023", Price = 19.49m, Stock = 22, Color = "Zielony", EstimatedHeight = 70, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Bluszcz", Sku = "5900000000024", Price = 15.99m, Stock = 50, Color = "Zielony", EstimatedHeight = 100, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Paproć Nephrolepis", Sku = "5900000000025", Price = 18.99m, Stock = 28, Color = "Zielony", EstimatedHeight = 45, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Dracena", Sku = "5900000000026", Price = 26.99m, Stock = 12, Color = "Zielono-biały", EstimatedHeight = 80, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Hortensja", Sku = "5900000000027", Price = 21.99m, Stock = 14, Color = "Różowy", EstimatedHeight = 50, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Żywotnik", Sku = "5900000000028", Price = 34.99m, Stock = 16, Color = "Zielony", EstimatedHeight = 150, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Róża miniaturowa", Sku = "5900000000029", Price = 15.49m, Stock = 40, Color = "Czerwony", EstimatedHeight = 35, Category = PredefinedCategories.IndoorOutdoor },

                    new Plant { Name = "Liliowiec", Sku = "5900000000030", Price = 13.99m, Stock = 30, Color = "Żółty", EstimatedHeight = 60, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Hosta", Sku = "5900000000031", Price = 16.99m, Stock = 25, Color = "Zielono-biały", EstimatedHeight = 50, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Grubosz", Sku = "5900000000032", Price = 17.49m, Stock = 35, Color = "Zielony", EstimatedHeight = 40, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Aglaonema", Sku = "5900000000033", Price = 22.99m, Stock = 20, Color = "Zielono-czerwony", EstimatedHeight = 45, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Kroton", Sku = "5900000000034", Price = 24.49m, Stock = 18, Color = "Wielokolorowy", EstimatedHeight = 55, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Cis", Sku = "5900000000035", Price = 28.99m, Stock = 10, Color = "Zielony", EstimatedHeight = 100, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Fiołek afrykański", Sku = "5900000000036", Price = 9.99m, Stock = 40, Color = "Fioletowy", EstimatedHeight = 15, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Geranium", Sku = "5900000000037", Price = 11.49m, Stock = 60, Color = "Czerwony", EstimatedHeight = 30, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Opuncja", Sku = "5900000000038", Price = 12.99m, Stock = 45, Color = "Zielony", EstimatedHeight = 25, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Rozchodnik", Sku = "5900000000039", Price = 10.99m, Stock = 50, Color = "Zielony", EstimatedHeight = 20, Category = PredefinedCategories.Outdoor },

                    new Plant { Name = "Wilczomlecz", Sku = "5900000000040", Price = 14.49m, Stock = 28, Color = "Zielono-czerwony", EstimatedHeight = 35, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Ripsalis", Sku = "5900000000041", Price = 19.99m, Stock = 18, Color = "Zielony", EstimatedHeight = 40, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Filodendron", Sku = "5900000000042", Price = 26.99m, Stock = 22, Color = "Zielony", EstimatedHeight = 60, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Kamelia", Sku = "5900000000043", Price = 22.49m, Stock = 15, Color = "Różowy", EstimatedHeight = 70, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Lobelia", Sku = "5900000000044", Price = 8.99m, Stock = 55, Color = "Niebieski", EstimatedHeight = 20, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Szałwia", Sku = "5900000000045", Price = 7.49m, Stock = 65, Color = "Zielony", EstimatedHeight = 25, Category = PredefinedCategories.IndoorOutdoor },
                    new Plant { Name = "Ananas ozdobny", Sku = "5900000000046", Price = 29.99m, Stock = 10, Color = "Zielono-żółty", EstimatedHeight = 50, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Calathea", Sku = "5900000000047", Price = 21.99m, Stock = 20, Color = "Zielono-biały", EstimatedHeight = 45, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Lilia", Sku = "5900000000048", Price = 18.49m, Stock = 25, Color = "Biały", EstimatedHeight = 60, Category = PredefinedCategories.Outdoor },
                    new Plant { Name = "Storczyk", Sku = "5900000000049", Price = 34.99m, Stock = 15, Color = "Różowy", EstimatedHeight = 50, Category = PredefinedCategories.Indoor },
                    new Plant { Name = "Difenbachia", Sku = "5900000000050", Price = 23.49m, Stock = 18, Color = "Zielono-biały", EstimatedHeight = 55, Category = PredefinedCategories.Indoor }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
