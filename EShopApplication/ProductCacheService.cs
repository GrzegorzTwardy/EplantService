using EShopDomain.Models;

namespace EShopApplication;

public interface IProductCacheService
{
    Task<IEnumerable<Product>> GetAllProductsAsync(Func<Task<IEnumerable<Product>>> getFromDatabase);
    Task<Product?> GetProductByIdAsync(int id, Func<int, Task<Product?>> getFromDatabase);
    Task InvalidateAllProductsCacheAsync();
    Task InvalidateProductCacheAsync(int productId);
}

public class ProductCacheService : IProductCacheService
{
    private const string AllProductsCacheKey = "all_products";
    private const string ProductCacheKeyPrefix = "product_";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24);

    private readonly IRedisCacheService _cache;

    public ProductCacheService(IRedisCacheService cache)
    {
        _cache = cache;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(Func<Task<IEnumerable<Product>>> getFromDatabase)
    {
        var cachedProducts = await _cache.GetAsync<IEnumerable<Product>>(AllProductsCacheKey);
        if (cachedProducts != null)
            return cachedProducts;

        var products = await getFromDatabase();
        await _cache.SetAsync(AllProductsCacheKey, products, CacheExpiration);
        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id, Func<int, Task<Product?>> getFromDatabase)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}{id}";
        var cachedProduct = await _cache.GetAsync<Product?>(cacheKey);

        if (cachedProduct != null)
            return cachedProduct;

        var product = await getFromDatabase(id);
        if (product != null) await _cache.SetAsync(cacheKey, product, CacheExpiration);

        return product;
    }

    public async Task InvalidateAllProductsCacheAsync()
    {
        await _cache.RemoveAsync(AllProductsCacheKey);
    }

    public async Task InvalidateProductCacheAsync(int productId)
    {
        var cacheKey = $"{ProductCacheKeyPrefix}{productId}";
        await _cache.RemoveAsync(cacheKey);
    }
}