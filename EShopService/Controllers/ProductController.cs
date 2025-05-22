using EShopApplication;
using EShopDomain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EShopService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/Product
    [HttpGet]
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _productService.GetAllProductsAsync();
    }

    // GET api/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    // POST api/Product
    [HttpPost]
    public async Task<ActionResult<Product>> PostAsync([FromBody] Product product)
    {
        await _productService.CreateProductAsync(product);
        return Created($"/api/Product/{product.Id}", product);
    }

    // PATCH api/Product
    [HttpPatch]
    public ActionResult<Product> Add([FromBody] Product product)
    {
        _productService.Add(product);
        return Created($"/api/Product/{product.Id}", product);
    }

    // PUT api/Product/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] Product product)
    {
        product.Id = id;
        await _productService.UpdateProductAsync(product);
        return NoContent();
    }

    // DELETE api/Product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}