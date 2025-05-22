using EShopApplication;
using EShopDomain.Models;
using EShopService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EShopServiceTests.Controllers;

public class ProductControllerTest
{
    private readonly ProductController _controller;
    private readonly Mock<IProductService> _mockProductService;

    public ProductControllerTest()
    {
        _mockProductService = new Mock<IProductService>();
        _controller = new ProductController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Price = 15.99m, Stock = 50 }
        };
        _mockProductService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProducts.Count, result.Count());
        Assert.Equal(expectedProducts, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsEmptyList_WhenNoProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>();
        _mockProductService.Setup(x => x.GetAllProductsAsync()).ReturnsAsync(expectedProducts);

        // Act
        var result = await _controller.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new Product { Id = productId, Name = "Test Product", Price = 19.99m, Stock = 25 };
        _mockProductService.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetAsync(productId);

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(expectedProduct, result.Value);
    }

    [Fact]
    public async Task GetAsync_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        _mockProductService.Setup(x => x.GetProductByIdAsync(productId)).ReturnsAsync((Product)null);

        // Act
        var result = await _controller.GetAsync(productId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task PostAsync_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var newProduct = new Product
        {
            Name = "New Product",
            Price = 29.99m,
            Stock = 75,
            Sku = "NP-001",
            Ean = "1234567890123"
        };

        _mockProductService.Setup(x => x.CreateProductAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 1)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PostAsync(newProduct);

        // Assert
        var createdResult = result.Result as CreatedResult;
        Assert.NotNull(createdResult);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("/api/Product/1", createdResult.Location);
        Assert.Equal(newProduct, createdResult.Value);
    }

    [Fact]
    public async Task PutAsync_WithValidProduct_SetsIdAndCallsUpdate()
    {
        // Arrange
        var productId = 1;
        var updatedProduct = new Product
        {
            Id = 0,
            Name = "Updated Product",
            Price = 39.99m,
            Stock = 30
        };

        _mockProductService.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(true);

        // Act
        await _controller.PutAsync(productId, updatedProduct);

        // Assert
        Assert.Equal(productId, updatedProduct.Id);
        _mockProductService.Verify(x => x.UpdateProductAsync(updatedProduct), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_CallsDeleteProduct()
    {
        // Arrange
        var productId = 1;
        _mockProductService.Setup(x => x.DeleteProductAsync(productId)).ReturnsAsync(true);

        // Act
        await _controller.DeleteAsync(productId);

        // Assert
        _mockProductService.Verify(x => x.DeleteProductAsync(productId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public async Task GetAsync_WithInvalidIds_ReturnsNotFound(int invalidId)
    {
        // Arrange
        _mockProductService.Setup(x => x.GetProductByIdAsync(invalidId)).ReturnsAsync((Product)null);

        // Act
        var result = await _controller.GetAsync(invalidId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(999)]
    public async Task PutAsync_WithVariousIds_SetsCorrectId(int productId)
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 0,
            Name = "Test Product",
            Price = 19.99m,
            Stock = 10
        };

        _mockProductService.Setup(x => x.UpdateProductAsync(It.IsAny<Product>())).ReturnsAsync(true);

        // Act
        await _controller.PutAsync(productId, updatedProduct);

        // Assert
        Assert.Equal(productId, updatedProduct.Id);
        _mockProductService.Verify(x => x.UpdateProductAsync(updatedProduct), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public async Task DeleteAsync_WithVariousIds_CallsDeleteProduct(int productId)
    {
        // Arrange
        _mockProductService.Setup(x => x.DeleteProductAsync(productId)).ReturnsAsync(true);

        // Act & Assert
        await _controller.DeleteAsync(productId);

        // Assert
        _mockProductService.Verify(x => x.DeleteProductAsync(productId), Times.Once);
    }
}