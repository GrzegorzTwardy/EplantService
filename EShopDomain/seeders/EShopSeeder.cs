using EShopDomain.Models;
using EShopDomain.Repositories;
using UserDomain.enums;
using UserDomain.Models;

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
        if (!_context.Users.Any()) SeedUsers();
    }

    private void SeedUsers()
    {
        var users = new List<User>
        {
            new()
            {
                CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid(), Username = "ala",
                Password = "ala", Role = UserRole.Admin
            },
            new()
            {
                CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid(), Username = "ma", Password = "ma",
                Role = UserRole.Employee
            },
            new()
            {
                CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid(), Username = "kota",
                Password = "kota", Role = UserRole.Client
            }
        };

        _context.Users.AddRange(users);
        _context.SaveChanges();
    }

    private void SeedCategories()
    {
        var categories = new List<Category>
        {
            new() { Name = "Indoor", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() },
            new() { Name = "Outdoor", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() },
            new() { Name = "IndoorAndOutdoor", CreatedBy = Guid.NewGuid(), UpdatedBy = Guid.NewGuid() }
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
				Name = "Fikus Benjamina",
				Ean = "5901234123460",
				Price = 29.99m,
				Stock = 40,
				Sku = "FB",
				CategoryId = categories[0].Id, // Indoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Lawenda",
				Ean = "5901234123461",
				Price = 22.50m,
				Stock = 60,
				Sku = "LW",
				CategoryId = categories[1].Id, // Outdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Aloes",
				Ean = "5901234123462",
				Price = 18.75m,
				Stock = 70,
				Sku = "AL",
				CategoryId = categories[2].Id, // IndoorAndOutdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Monstera Deliciosa",
				Ean = "5901234123463",
				Price = 55.00m,
				Stock = 25,
				Sku = "MD",
				CategoryId = categories[0].Id, // Indoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Hortensja",
				Ean = "5901234123464",
				Price = 35.99m,
				Stock = 45,
				Sku = "HT",
				CategoryId = categories[1].Id, // Outdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Begonia",
				Ean = "5901234123465",
				Price = 27.30m,
				Stock = 55,
				Sku = "BG",
				CategoryId = categories[2].Id, // IndoorAndOutdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Sansewieria",
				Ean = "5901234123466",
				Price = 40.00m,
				Stock = 50,
				Sku = "SW",
				CategoryId = categories[0].Id, // Indoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Róża ogrodowa",
				Ean = "5901234123467",
				Price = 33.99m,
				Stock = 70,
				Sku = "RO",
				CategoryId = categories[1].Id, // Outdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Bluszcz pospolity",
				Ean = "5901234123468",
				Price = 24.50m,
				Stock = 60,
				Sku = "BP",
				CategoryId = categories[2].Id, // IndoorAndOutdoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			},
			new()
			{
				Name = "Paprotka zwyczajna",
				Ean = "5901234123469",
				Price = 20.00m,
				Stock = 80,
				Sku = "PZ",
				CategoryId = categories[0].Id, // Indoor
				CreatedBy = Guid.NewGuid(),
				UpdatedBy = Guid.NewGuid()
			}
		};

		_context.Products.AddRange(products);
		_context.SaveChanges();
	}
}