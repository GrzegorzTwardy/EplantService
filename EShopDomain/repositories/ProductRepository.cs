using EShopDomain.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopDomain.Repositories;

public interface IProductRepository
{
    IEnumerable<Product> GetAll();
    Product? GetById(int id);
    void Add(Product product);
    void Update(Product product);
    void Delete(int id);
    bool Exists(int id);
}

public class ProductRepository : IProductRepository
{
    private readonly DataContext _context;

    public ProductRepository(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<Product> GetAll()
    {
        return _context.Products.Include(p => p.Category).ToList();
    }

    public Product? GetById(int id)
    {
        return _context.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id);
    }

    public void Add(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void Update(Product product)
    {
        var existingProduct = _context.Products.Find(product.Id);
        if (existingProduct == null) throw new KeyNotFoundException($"Product with ID {product.Id} not found");

        // Check if the category exists if CategoryId is being changed
        if (product.CategoryId != existingProduct.CategoryId)
        {
            var categoryExists = _context.Categories.Any(c => c.Id == product.CategoryId);
            if (!categoryExists) throw new KeyNotFoundException($"Category with ID {product.CategoryId} not found");
        }

        // Update properties individually
        _context.Entry(existingProduct).CurrentValues.SetValues(product);

        if (product.CategoryId != existingProduct.CategoryId)
            // Remove reference to old Category object
            existingProduct.Category = null;

        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            product.Deleted = true;
            _context.SaveChanges();
        }
    }

    public bool Exists(int id)
    {
        return _context.Products.Any(p => p.Id == id);
    }
}