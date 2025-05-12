using EShopDomain.Models;
using EShopDomain.Repositories;

namespace EShopApplication;

public interface IProductService
{
    IEnumerable<Product> GetAllProducts();
    Product? GetProductById(int id);
    void CreateProduct(Product product);
    bool UpdateProduct(Product product);
    bool DeleteProduct(int id);
}

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public IEnumerable<Product> GetAllProducts()
    {
        return _productRepository.GetAll();
    }

    public Product? GetProductById(int id)
    {
        return _productRepository.GetById(id);
    }

    public void CreateProduct(Product product)
    {
        // Set default values for audit fields
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        product.CreatedBy = Guid.NewGuid();
        product.UpdatedBy = Guid.NewGuid();

        _productRepository.Add(product);
    }

    public bool UpdateProduct(Product product)
    {
        var existingProduct = _productRepository.GetById(product.Id);
        if (existingProduct == null)
            return false;

        // Update audit fields
        product.UpdatedAt = DateTime.UtcNow;
        product.UpdatedBy = Guid.NewGuid();
        product.CreatedAt = existingProduct.CreatedAt;
        product.CreatedBy = existingProduct.CreatedBy;

        _productRepository.Update(product);
        return true;
    }

    public bool DeleteProduct(int id)
    {
        if (!_productRepository.Exists(id))
            return false;

        _productRepository.Delete(id);
        return true;
    }
}