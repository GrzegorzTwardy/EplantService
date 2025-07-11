using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using EShopApplication;
using EShopApplicationTests;
using EShopDomain.Models;
using EShopDomain.Repositories;
using EShopService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit.Abstractions;

namespace EShopServiceIntegrationTests.Controllers;

public class ProductControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public ProductControllerIntegrationTest(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _factory = factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove the existing database context
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<DataContext>));

                    if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                    // Remove Redis services
                    var redisMultiplexerDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(IConnectionMultiplexer));
                    if (redisMultiplexerDescriptor != null) services.Remove(redisMultiplexerDescriptor);

                    var redisCacheServiceDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(IRedisCacheService));
                    if (redisCacheServiceDescriptor != null) services.Remove(redisCacheServiceDescriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<DataContext>(options => { options.UseInMemoryDatabase("TestDB"); });

                    // Add in-memory cache service for testing
                    services.AddSingleton<IRedisCacheService, InMemoryCacheService>();
                });
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Add_AddProduct_ExceptedOneProduct()
    {
        // Arrange
        var category = await SeedCategory();
        var newProduct = new Product
        {
            Name = "PATCH Test Product",
            Ean = "1111222233334",
            Price = 49.99m,
            Stock = 75,
            Sku = "PATCH-001",
            CategoryId = category.Id
        };

        // Act
        var response = await PatchProductRequest(newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(createdProduct.TryGetProperty("id", out var id));
        Assert.True(id.GetInt32() > 0);
        Assert.True(createdProduct.TryGetProperty("name", out var name));
        Assert.Equal("PATCH Test Product", name.GetString());

        // Verify Location header is set
        Assert.NotNull(response.Headers.Location);

        // Verify the product was actually added to the database
        var getAllResponse = await GetAllProductsRequest();
        var allProductsContent = await getAllResponse.Content.ReadAsStringAsync();
        var allProducts = JsonSerializer.Deserialize<List<JsonElement>>(allProductsContent);

        Assert.NotNull(allProducts);
        Assert.Single(allProducts);

        var addedProduct = allProducts[0];
        Assert.True(addedProduct.TryGetProperty("name", out var addedName));
        Assert.Equal("PATCH Test Product", addedName.GetString());
        Assert.True(addedProduct.TryGetProperty("sku", out var addedSku));
        Assert.Equal("PATCH-001", addedSku.GetString());
    }

    private async Task<HttpResponseMessage> PatchProductRequest(Product product)
    {
        var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PatchAsync("/api/product", content);
    }

    [Fact]
    public async Task Post_AddThousandsProductsAsync_ExceptedThousandsProducts()
    {
        //  Total time: 745 ms (0.75 seconds)
        // Arrange
        var category = await SeedCategory();
        const int productCount = 10000;
        const int batchSize = 100; // Process in batches to avoid overwhelming the database
        var stopwatch = Stopwatch.StartNew();

        _testOutputHelper.WriteLine(
            $"Starting ASYNC CONCURRENT test - Adding {productCount} products using List<Task>...");

        // Act - Using List<Task> for concurrent operations
        var totalProcessed = 0;

        for (var batchStart = 1; batchStart <= productCount; batchStart += batchSize)
        {
            var tasks = new List<Task>();
            var currentBatchEnd = Math.Min(batchStart + batchSize - 1, productCount);

            for (var i = batchStart; i <= currentBatchEnd; i++)
            {
                var productIndex = i; // Capture loop variable

                var task = Task.Run(async () =>
                {
                    var product = new Product
                    {
                        Name = $"Async Concurrent Product {productIndex}",
                        Ean = $"{1000000000000 + productIndex:D13}",
                        Price = 19.99m + productIndex % 100,
                        Stock = 100 + productIndex % 50,
                        Sku = $"ASYNC-CONC-{productIndex:D5}",
                        CategoryId = category.Id
                    };

                    var response = await PostProductRequest(product);
                    if (response.StatusCode != HttpStatusCode.Created)
                        _testOutputHelper.WriteLine($"Failed to create product {productIndex}: {response.StatusCode}");
                });

                tasks.Add(task);
            }

            // Wait for all tasks in this batch to complete
            await Task.WhenAll(tasks);

            totalProcessed = currentBatchEnd;
            _testOutputHelper.WriteLine(
                $"ASYNC CONCURRENT: Completed batch ending at {currentBatchEnd} ({stopwatch.ElapsedMilliseconds}ms elapsed)");
        }

        stopwatch.Stop();

        // Assert
        var getAllResponse = await GetAllProductsRequest();
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var responseContent = await getAllResponse.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.NotNull(products);
        Assert.True(products.Count >= productCount,
            $"Expected at least {productCount} products, but found {products.Count}");

        // Print performance results
        _testOutputHelper.WriteLine("ASYNC CONCURRENT TEST COMPLETED:");
        _testOutputHelper.WriteLine($"  Products added: {productCount}");
        _testOutputHelper.WriteLine($"  Batch size: {batchSize}");
        _testOutputHelper.WriteLine(
            $"  Total time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
        _testOutputHelper.WriteLine(
            $"  Average time per product: {(double)stopwatch.ElapsedMilliseconds / productCount:F2} ms");
        _testOutputHelper.WriteLine($"  Products per second: {productCount / stopwatch.Elapsed.TotalSeconds:F2}");
    }

    [Fact]
    public void Post_AddThousandsProducts_ExceptedThousandsProducts()
    {
        // Total time: 6112 ms (6.11 seconds)
        // Arrange
        var category = SeedCategory().GetAwaiter().GetResult();
        const int productCount = 10000;
        var stopwatch = Stopwatch.StartNew();

        _testOutputHelper.WriteLine($"Starting PURE SYNC test - Adding {productCount} products sequentially...");

        // Act - Pure synchronous, sequential operations
        for (var i = 1; i <= productCount; i++)
        {
            var product = new Product
            {
                Name = $"Pure Sync Product {i}",
                Ean = $"{3000000000000 + i:D13}",
                Price = 39.99m + i % 100,
                Stock = 300 + i % 50,
                Sku = $"PURE-SYNC-{i:D5}",
                CategoryId = category.Id
            };

            var response = PostProductRequest(product).GetAwaiter().GetResult();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            if (i % 1000 == 0)
                _testOutputHelper.WriteLine(
                    $"PURE SYNC: Added {i} products so far... ({stopwatch.ElapsedMilliseconds}ms elapsed)");
        }

        stopwatch.Stop();

        // Assert
        var getAllResponse = GetAllProductsRequest().GetAwaiter().GetResult();
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);

        var responseContent = getAllResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var products = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.NotNull(products);
        Assert.True(products.Count >= productCount,
            $"Expected at least {productCount} products, but found {products.Count}");

        // Print performance results
        _testOutputHelper.WriteLine("PURE SYNC TEST COMPLETED:");
        _testOutputHelper.WriteLine($"  Products added: {productCount}");
        _testOutputHelper.WriteLine(
            $"  Total time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
        _testOutputHelper.WriteLine(
            $"  Average time per product: {(double)stopwatch.ElapsedMilliseconds / productCount:F2} ms");
        _testOutputHelper.WriteLine($"  Products per second: {productCount / stopwatch.Elapsed.TotalSeconds:F2}");
    }

    private async Task<HttpResponseMessage> GetAllProductsRequest()
    {
        return await _client.GetAsync("/api/product");
    }

    private async Task<HttpResponseMessage> GetProductByIdRequest(int id)
    {
        return await _client.GetAsync($"/api/product/{id}");
    }

    private async Task<HttpResponseMessage> PostProductRequest(Product product)
    {
        var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/api/product", content);
    }

    private async Task<HttpResponseMessage> PutProductRequest(int id, Product product)
    {
        var json = JsonSerializer.Serialize(product, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PutAsync($"/api/product/{id}", content);
    }

    private async Task<HttpResponseMessage> DeleteProductRequest(int id)
    {
        return await _client.DeleteAsync($"/api/product/{id}");
    }

    private async Task<Category> SeedCategory()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        // Ensure database is created
        context.Database.EnsureCreated();

        // Clear existing data
        context.Products.RemoveRange(context.Products);
        context.Categories.RemoveRange(context.Categories);
        await context.SaveChangesAsync();

        // Add test category
        var category = new Category
        {
            Name = "Test Category",
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        context.Entry(category).State = EntityState.Detached;

        return category;
    }

    private async Task<Product> SeedProduct(int categoryId)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();

        var product = new Product
        {
            Name = "Test Product",
            Ean = "1234567890123",
            Price = 19.99m,
            Stock = 100,
            Sku = "TST-001",
            CategoryId = categoryId,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();
        context.Entry(product).State = EntityState.Detached;
        return product;
    }

    [Fact]
    public async Task Get_WithNoProducts_ReturnsEmptyList()
    {
        // Arrange - Clean database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        context.Database.EnsureCreated();
        context.Products.RemoveRange(context.Products);
        await context.SaveChangesAsync();

        // Act
        var response = await GetAllProductsRequest();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task Get_WithProducts_ReturnsAllProducts()
    {
        // Arrange
        var category = await SeedCategory();
        var product = await SeedProduct(category.Id);

        // Act
        var response = await GetAllProductsRequest();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);

        Assert.NotNull(products);
        Assert.Single(products);

        var returnedProduct = products[0];
        Assert.True(returnedProduct.TryGetProperty("id", out var id));
        Assert.Equal(product.Id, id.GetInt32());
        Assert.True(returnedProduct.TryGetProperty("name", out var name));
        Assert.Equal("Test Product", name.GetString());
    }

    [Fact]
    public async Task Get_WithValidId_ReturnsProduct()
    {
        // Arrange
        var category = await SeedCategory();
        var product = await SeedProduct(category.Id);

        // Act
        var response = await GetProductByIdRequest(product.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(returnedProduct.TryGetProperty("id", out var id));
        Assert.Equal(product.Id, id.GetInt32());
        Assert.True(returnedProduct.TryGetProperty("name", out var name));
        Assert.Equal("Test Product", name.GetString());
        Assert.True(returnedProduct.TryGetProperty("price", out var price));
        Assert.Equal(19.99m, price.GetDecimal());
    }

    [Fact]
    public async Task Get_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var response = await GetProductByIdRequest(nonExistentId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidProduct_ReturnsCreatedAtAction()
    {
        // Arrange
        var category = await SeedCategory();
        var newProduct = new Product
        {
            Name = "New Product",
            Ean = "9876543210987",
            Price = 39.99m,
            Stock = 25,
            Sku = "NEW-001",
            CategoryId = category.Id
        };

        // Act
        var response = await PostProductRequest(newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(createdProduct.TryGetProperty("id", out var id));
        Assert.True(id.GetInt32() > 0);
        Assert.True(createdProduct.TryGetProperty("name", out var name));
        Assert.Equal("New Product", name.GetString());

        // Verify Location header is set (CreatedAtAction sets this)
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Post_WithValidProduct_SetsAuditFields()
    {
        // Arrange
        var category = await SeedCategory();
        var newProduct = new Product
        {
            Name = "Audit Test Product",
            Ean = "1111111111111",
            Price = 15.00m,
            Stock = 10,
            Sku = "AUD-001",
            CategoryId = category.Id
        };

        // Act
        var response = await PostProductRequest(newProduct);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(responseContent);

        // Verify audit fields are set (ProductService sets these)
        Assert.True(createdProduct.TryGetProperty("createdAt", out var createdAt));
        Assert.NotEqual(DateTime.MinValue, createdAt.GetDateTime());
        Assert.True(createdProduct.TryGetProperty("updatedAt", out var updatedAt));
        Assert.NotEqual(DateTime.MinValue, updatedAt.GetDateTime());
        Assert.True(createdProduct.TryGetProperty("createdBy", out var createdBy));
        Assert.NotEqual(Guid.Empty, createdBy.GetGuid());
        Assert.True(createdProduct.TryGetProperty("updatedBy", out var updatedBy));
        Assert.NotEqual(Guid.Empty, updatedBy.GetGuid());
    }

    [Fact]
    public async Task Put_WithValidProduct_ReturnsNoContent()
    {
        // Arrange
        var category = await SeedCategory();
        var existingProduct = await SeedProduct(category.Id);

        var updatedProduct = new Product
        {
            Id = 999, // This will be overridden by the controller
            Name = "Updated Product",
            Ean = "9999999999999",
            Price = 99.99m,
            Stock = 200,
            Sku = "UPD-001",
            CategoryId = category.Id
        };

        // Act
        var response = await PutProductRequest(existingProduct.Id, updatedProduct);

        // Assert - PUT returns void (204 No Content)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Put_OverridesProductIdWithRouteId()
    {
        // Arrange
        var category = await SeedCategory();
        var existingProduct = await SeedProduct(category.Id);

        var updatedProduct = new Product
        {
            Id = 999, // Different from route ID
            Name = "ID Override Test",
            Ean = existingProduct.Ean,
            Price = 55.55m,
            Stock = 75,
            Sku = existingProduct.Sku,
            CategoryId = category.Id
        };

        // Act
        var response = await PutProductRequest(existingProduct.Id, updatedProduct);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the product was updated with the route ID, not the body ID
        var getResponse = await GetProductByIdRequest(existingProduct.Id);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var responseContent = await getResponse.Content.ReadAsStringAsync();
        var retrievedProduct = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(retrievedProduct.TryGetProperty("id", out var id));
        Assert.Equal(existingProduct.Id, id.GetInt32()); // Should be route ID
        Assert.True(retrievedProduct.TryGetProperty("name", out var name));
        Assert.Equal("ID Override Test", name.GetString());
    }

    [Fact]
    public async Task Put_WithNonExistentProduct_StillReturnsNoContent()
    {
        // Arrange
        var category = await SeedCategory();
        var nonExistentId = 999;

        var product = new Product
        {
            Id = nonExistentId,
            Name = "Non-existent Product",
            Ean = "0000000000000",
            Price = 10.00m,
            Stock = 5,
            Sku = "NON-001",
            CategoryId = category.Id
        };

        // Act
        var response = await PutProductRequest(nonExistentId, product);

        // Assert - Controller doesn't check if update was successful, always returns void
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var category = await SeedCategory();
        var existingProduct = await SeedProduct(category.Id);

        // Act
        var response = await DeleteProductRequest(existingProduct.Id);

        // Assert - DELETE returns void (204 No Content)
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNoContent()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var response = await DeleteProductRequest(nonExistentId);

        // Assert - Controller doesn't check if delete was successful, always returns void
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SoftDeletesProduct()
    {
        // Arrange
        var category = await SeedCategory();
        var existingProduct = await SeedProduct(category.Id);

        // Act
        var deleteResponse = await DeleteProductRequest(existingProduct.Id);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify product is no longer returned (due to soft delete query filter)
        var getResponse = await GetProductByIdRequest(existingProduct.Id);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        // Verify product is not in GetAll results
        var getAllResponse = await GetAllProductsRequest();
        var responseContent = await getAllResponse.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<JsonElement>>(responseContent);
        Assert.Empty(products);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public async Task Get_ByInvalidId_ReturnsNotFound(int invalidId)
    {
        // Act
        var response = await GetProductByIdRequest(invalidId);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithNullProduct_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/product", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/product", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ProductController_EndpointsExist_AllMethodsAccessible()
    {
        // Test that all endpoints are accessible (not returning 404 Method Not Found)

        // GET /api/product
        var getAllResponse = await _client.GetAsync("/api/product");
        Assert.NotEqual(HttpStatusCode.NotFound, getAllResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, getAllResponse.StatusCode);

        // GET /api/product/1
        var getByIdResponse = await _client.GetAsync("/api/product/1");
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, getByIdResponse.StatusCode);

        // POST /api/product
        var postContent = new StringContent("{}", Encoding.UTF8, "application/json");
        var postResponse = await _client.PostAsync("/api/product", postContent);
        Assert.NotEqual(HttpStatusCode.NotFound, postResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, postResponse.StatusCode);

        // PUT /api/product/1
        var putContent = new StringContent("{}", Encoding.UTF8, "application/json");
        var putResponse = await _client.PutAsync("/api/product/1", putContent);
        Assert.NotEqual(HttpStatusCode.NotFound, putResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, putResponse.StatusCode);

        // DELETE /api/product/1
        var deleteResponse = await _client.DeleteAsync("/api/product/1");
        Assert.NotEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.NotEqual(HttpStatusCode.MethodNotAllowed, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task FullCrudWorkflow_WorksAsExpected()
    {
        // Arrange
        var category = await SeedCategory();
        var testProduct = new Product
        {
            Name = "CRUD Test Product",
            Ean = "1234567890123",
            Price = 25.50m,
            Stock = 50,
            Sku = "CRUD-001",
            CategoryId = category.Id
        };

        // Act & Assert - CREATE
        var createResponse = await PostProductRequest(testProduct);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createContent = await createResponse.Content.ReadAsStringAsync();
        var createdProduct = JsonSerializer.Deserialize<JsonElement>(createContent);
        Assert.True(createdProduct.TryGetProperty("id", out var productIdElement));
        var productId = productIdElement.GetInt32();

        // Ensure we got a valid ID
        Assert.True(productId > 0, $"Expected positive product ID, got {productId}");

        // Act & Assert - READ
        var readResponse = await GetProductByIdRequest(productId);

        // Debug: If this fails, let's see what products exist
        if (readResponse.StatusCode != HttpStatusCode.OK)
        {
            var getAllResponse = await GetAllProductsRequest();
            var allProductsContent = await getAllResponse.Content.ReadAsStringAsync();
            var allProducts = JsonSerializer.Deserialize<List<JsonElement>>(allProductsContent);

            // This will show us what products actually exist in the database
            Assert.True(false, $"Could not find product with ID {productId}. " +
                               $"Available products: {string.Join(", ", allProducts.Select(p => p.GetProperty("id").GetInt32()))}. " +
                               $"Total products in DB: {allProducts.Count}");
        }

        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        // Act & Assert - UPDATE
        var updateProduct = new Product
        {
            Name = "Updated CRUD Product",
            Ean = "9876543210987",
            Price = 99.99m,
            Stock = 25,
            Sku = "CRUD-UPD",
            CategoryId = category.Id
        };

        var updateResponse = await PutProductRequest(productId, updateProduct);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Act & Assert - DELETE
        var deleteResponse = await DeleteProductRequest(productId);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var verifyDeleteResponse = await GetProductByIdRequest(productId);
        Assert.Equal(HttpStatusCode.NotFound, verifyDeleteResponse.StatusCode);
    }
}