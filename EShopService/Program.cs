using EShopApplication;
using EShopDomain.Repositories;
using EShopDomain.seeders;
using Microsoft.EntityFrameworkCore;

namespace EShopService;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Twój dotychczasowy kod

        var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
        builder.Services.AddControllers();

// Add database context with hardcoded connection string
        builder.Services.AddDbContext<DataContext>(options =>
            options.UseNpgsql("Host=postgres;Database=eshop;Username=admin;Password=admin"));

// Register repositories
        builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register services
        builder.Services.AddScoped<IProductService, ProductService>();

// Register card validation services
        builder.Services.AddScoped<ICardLengthService, CardLengthService>();
        builder.Services.AddScoped<ICardService, CardService>();

// Register seeder
        builder.Services.AddScoped<IEShopSeeder, EShopSeeder>();

// Add Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // Ensure database is created before seeding
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                // Ensure database is created
                dbContext.Database.EnsureCreated();

                if (!app.Environment.IsEnvironment("Testing"))
                {
                    // Seed the database
                    var seeder = scope.ServiceProvider.GetRequiredService<IEShopSeeder>();
                    seeder.Seed();
                }
            }
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}