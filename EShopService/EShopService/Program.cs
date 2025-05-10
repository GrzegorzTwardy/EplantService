using EShop.Application.Service;
using EShop.Application.Services;
using EShop.Domain.Repositories;
using EShop.Domain.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace EShopService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");


            //builder.Services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase("TestDb"), ServiceLifetime.Transient);
            builder.Services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Transient);
            builder.Services.AddScoped<IRepository, Repository>();


            // Add services to the container.
            builder.Services.AddScoped<ICreditCardService, CreditCardService>();
            builder.Services.AddScoped<IProductService, ProductService>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            builder.Services.AddScoped<IEShopSeeder, EShopSeeder>();
            builder.Services.AddMemoryCache();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();


            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                await db.Database.MigrateAsync();
                var seeder = scope.ServiceProvider.GetRequiredService<IEShopSeeder>();
                await seeder.Seed();
            }

            app.Run();
        }
    }

}