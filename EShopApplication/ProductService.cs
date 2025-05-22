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
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task CreateProductAsync(Product product)
    {
        // Set default values for audit fields
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.CreatedBy = Guid.NewGuid();
        product.UpdatedBy = Guid.NewGuid();

        await _productRepository.AddAsync(product);
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
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        if (!await _productRepository.ExistsAsync(id))
            return false;

        await _productRepository.DeleteAsync(id);
        return true;
    }
}