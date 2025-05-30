using EShopDomain.Models;
using EShopDomain.Repositories;

namespace EShopApplication;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task CreateProductAsync(Product product);
    void Add(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IProductCacheService _cacheService;
    private readonly IProductRepository _productRepository;

    public ProductService(
        IProductRepository productRepository,
        IProductCacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _cacheService.GetAllProductsAsync(async () => { return await _productRepository.GetAllAsync(); });
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _cacheService.GetProductByIdAsync(id,
            async productId => { return await _productRepository.GetByIdAsync(productId); });
    }

    public async Task CreateProductAsync(Product product)
    {
        // Set default values for audit fields
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.CreatedBy = Guid.NewGuid();
        product.UpdatedBy = Guid.NewGuid();

        await _productRepository.AddAsync(product);

        // Invalidate cache after creating a new product
        await _cacheService.InvalidateAllProductsCacheAsync();
    }

    public void Add(Product product)
    {
        // Set default values for audit fields
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.CreatedBy = Guid.NewGuid();
        product.UpdatedBy = Guid.NewGuid();

        // Use GetAwaiter().GetResult() to make async call synchronous
        _productRepository.AddAsync(product).GetAwaiter().GetResult();

        // Invalidate cache after adding a new product
        _cacheService.InvalidateAllProductsCacheAsync();
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        var existingProduct = await _productRepository.GetByIdAsync(product.Id);
        if (existingProduct == null)
            return false;

        // Update audit fields
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = Guid.NewGuid();
        product.CreatedAt = existingProduct.CreatedAt;
        product.CreatedBy = existingProduct.CreatedBy;

        await _productRepository.UpdateAsync(product);

        // Invalidate cache after updating the product
        _cacheService.InvalidateProductCacheAsync(product.Id);
        _cacheService.InvalidateAllProductsCacheAsync();

        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        if (!await _productRepository.ExistsAsync(id))
            return false;

        await _productRepository.DeleteAsync(id);

        // Invalidate cache after deleting the product
        _cacheService.InvalidateProductCacheAsync(id);
        _cacheService.InvalidateAllProductsCacheAsync();

        return true;
    }
}