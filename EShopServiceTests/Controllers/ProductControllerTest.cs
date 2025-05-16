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
    public void Get_ReturnsAllProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, Stock = 100 },
            new() { Id = 2, Name = "Product 2", Price = 15.99m, Stock = 50 }
        };
        _mockProductService.Setup(x => x.GetAllProducts()).Returns(expectedProducts);

        // Act
        var result = _controller.Get();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProducts.Count, result.Count());
        Assert.Equal(expectedProducts, result);
    }

    [Fact]
    public void Get_ReturnsEmptyList_WhenNoProducts()
    {
        // Arrange
        var expectedProducts = new List<Product>();
        _mockProductService.Setup(x => x.GetAllProducts()).Returns(expectedProducts);

        // Act
        var result = _controller.Get();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Get_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new Product { Id = productId, Name = "Test Product", Price = 19.99m, Stock = 25 };
        _mockProductService.Setup(x => x.GetProductById(productId)).Returns(expectedProduct);

        // Act
        var result = _controller.Get(productId);

        // Assert
        Assert.NotNull(result.Value);
        Assert.Equal(expectedProduct, result.Value);
    }

    [Fact]
    public void Get_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        _mockProductService.Setup(x => x.GetProductById(productId)).Returns((Product)null);

        // Act
        var result = _controller.Get(productId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Null(result.Value);
    }

    [Fact]
    public void Post_WithValidProduct_ReturnsCreatedAtAction()
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

        _mockProductService.Setup(x => x.CreateProduct(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 1);

        // Act
        var result = _controller.Post(newProduct);

        // Assert
        var createdAtActionResult = result.Result as CreatedAtActionResult;
        Assert.NotNull(createdAtActionResult);
        Assert.Equal(201, createdAtActionResult.StatusCode);
        Assert.Equal(nameof(_controller.Get), createdAtActionResult.ActionName);
    }

    [Fact]
    public void Put_WithValidProduct_SetsIdAndCallsUpdate()
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

        // Act
        _controller.Put(productId, updatedProduct);

        // Assert
        Assert.Equal(productId, updatedProduct.Id);
        _mockProductService.Verify(x => x.UpdateProduct(updatedProduct), Times.Once);
    }

    [Fact]
    public void Delete_WithValidId_CallsDeleteProduct()
    {
        // Arrange
        var productId = 1;

        // Act
        _controller.Delete(productId);

        // Assert
        _mockProductService.Verify(x => x.DeleteProduct(productId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(999)]
    public void Get_WithInvalidIds_ReturnsNotFound(int invalidId)
    {
        // Arrange
        _mockProductService.Setup(x => x.GetProductById(invalidId)).Returns((Product)null);

        // Act
        var result = _controller.Get(invalidId);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.NotNull(notFoundResult);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(999)]
    public void Put_WithVariousIds_SetsCorrectId(int productId)
    {
        // Arrange
        var updatedProduct = new Product
        {
            Id = 0,
            Name = "Test Product",
            Price = 19.99m,
            Stock = 10
        };

        // Act
        _controller.Put(productId, updatedProduct);

        // Assert
        Assert.Equal(productId, updatedProduct.Id);
        _mockProductService.Verify(x => x.UpdateProduct(updatedProduct), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Delete_WithVariousIds_CallsDeleteProduct(int productId)
    {
        // Arrange & Act
        _controller.Delete(productId);

        // Assert
        _mockProductService.Verify(x => x.DeleteProduct(productId), Times.Once);
    }
}