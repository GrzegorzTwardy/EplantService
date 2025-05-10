using EShop.Domain.Repositories;
using EShopDomain.Models;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

namespace EShop.Application.Service
{
    public class ProductService : IProductService
    {
        private IRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly IDatabase _redisDb;


        public ProductService(IRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            _redisDb = redis.GetDatabase();
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var result = await _repository.GetAllProductAsync();

            return result;
        }

        public async Task<Product> GetAsync(int id)
        {
            string key = $"Product:{id}";
            var product = JsonSerializer.Deserialize<Product>(await _redisDb.StringGetAsync(key));

            if (product == null)
            {
                    product = await _repository.GetProductAsync(id);
                    await _redisDb.StringSetAsync(key, JsonSerializer.Serialize(product), TimeSpan.FromDays(1));
            }

            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            var result = await _repository.UpdateProductAsync(product);

            string key = $"Product:{product.Id}";
            await _redisDb.KeyDeleteAsync(key);

            return result;
        }

        public async Task<Product> AddAsync(Product product)
        {
             var result =  await _repository.AddProductAsync(product);

            return result;
        }

        public Product Add(Product product)
        {
            var result = _repository.AddProductAsync(product).Result;

            return result;
        }
    }
}
