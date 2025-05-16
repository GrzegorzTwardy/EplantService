using System.Net;
using System.Text;
using System.Text.Json;
using EShopDomain.Repositories;
using EShopService;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EShopServiceIntegrationTests.Controllers;

public class CardValidationControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public CardValidationControllerIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing database context
                    var dbContextDescriptor = services.SingleOrDefault(d =>
                        d.ServiceType == typeof(DbContextOptions<DataContext>));

                    if (dbContextDescriptor != null) services.Remove(dbContextDescriptor);

                    // Add in-memory database for testing
                    services.AddDbContext<DataContext>(options => { options.UseInMemoryDatabase("TestDatabase"); });
                });
            });

        _client = _factory.CreateClient();
    }

    private async Task<HttpResponseMessage> PostCardValidationRequest(string cardNumber)
    {
        var json = JsonSerializer.Serialize(cardNumber);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync("/api/cardvalidation", content);
    }

    [Fact]
    public async Task ValidateCard_WithValidVisaCard_ReturnsOk()
    {
        // Arrange - Valid Visa card number (16 digits, passes Luhn algorithm)
        var validVisaCard = "4532015112830366";

        // Act
        var response = await PostCardValidationRequest(validVisaCard);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Card validation successful", message.GetString());
        Assert.True(responseData.TryGetProperty("cardType", out var cardType));
        Assert.Equal("Visa", cardType.GetString());
    }

    [Fact]
    public async Task ValidateCard_WithValidMasterCard_ReturnsOk()
    {
        // Arrange - Valid MasterCard number (16 digits, passes Luhn algorithm)
        var validMasterCard = "5555555555554444";

        // Act
        var response = await PostCardValidationRequest(validMasterCard);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Card validation successful", message.GetString());
        Assert.True(responseData.TryGetProperty("cardType", out var cardType));
        Assert.Equal("MasterCard", cardType.GetString());
    }

    [Fact]
    public async Task ValidateCard_WithInvalidChecksum_ReturnsBadRequest()
    {
        // Arrange - Card with invalid Luhn checksum
        var invalidCard = "4532015112830367"; // Last digit changed to make checksum invalid

        // Act
        var response = await PostCardValidationRequest(invalidCard);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
        Assert.Contains("failed validation check", error.GetString());
    }

    [Fact]
    public async Task ValidateCard_WithTooShortCard_ReturnsBadRequest()
    {
        // Arrange - Card number too short
        var shortCard = "123456789";

        // Act
        var response = await PostCardValidationRequest(shortCard);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
        Assert.True(responseData.TryGetProperty("minLength", out var minLength));
        Assert.True(responseData.TryGetProperty("actualLength", out var actualLength));
        Assert.Equal(9, actualLength.GetInt32());
    }

    [Fact]
    public async Task ValidateCard_WithTooLongCard_ReturnsUriTooLong()
    {
        // Arrange - Card number too long (over 19 digits)
        var longCard = "12345678901234567890123";

        // Act
        var response = await PostCardValidationRequest(longCard);

        // Assert
        Assert.Equal(HttpStatusCode.RequestUriTooLong, response.StatusCode); // 414

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
        Assert.True(responseData.TryGetProperty("maxLength", out var maxLength));
        Assert.True(responseData.TryGetProperty("actualLength", out var actualLength));
        Assert.Equal(23, actualLength.GetInt32());
    }

    [Fact]
    public async Task ValidateCard_WithUnsupportedCardType_ReturnsNotAcceptable()
    {
        // Arrange - Valid card number but unsupported type (e.g., Diners Club)
        // This assumes Diners Club is not in the CreditCardProvider enum
        var dinersCard = "30569309025904"; // Valid Diners Club number

        // Act
        var response = await PostCardValidationRequest(dinersCard);

        // Assert
        Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode); // 406

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
        Assert.Contains("not supported", error.GetString());
    }

    [Fact]
    public async Task ValidateCard_WithInvalidFormat_ReturnsBadRequest()
    {
        // Arrange - Card with non-numeric characters
        var invalidFormatCard = "4532-0151-1283-0366";

        // Act
        var response = await PostCardValidationRequest(invalidFormatCard);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
    }

    [Fact]
    public async Task ValidateCard_WithEmptyString_ReturnsBadRequest()
    {
        // Arrange
        var emptyCard = "";

        // Act
        var response = await PostCardValidationRequest(emptyCard);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out var error));
    }

    [Fact]
    public async Task ValidateCard_WithValidAmexCard_ReturnsOk()
    {
        // Arrange - Valid American Express card (15 digits)
        var validAmexCard = "378282246310005";

        // Act
        var response = await PostCardValidationRequest(validAmexCard);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Card validation successful", message.GetString());
        Assert.True(responseData.TryGetProperty("cardType", out var cardType));
        Assert.Equal("AmericanExpress", cardType.GetString());
    }

    [Theory]
    [InlineData("4532015112830366")] // Visa
    [InlineData("5555555555554444")] // MasterCard
    [InlineData("378282246310005")] // American Express
    public async Task ValidateCard_WithMultipleValidCards_ReturnsOk(string cardNumber)
    {
        // Act
        var response = await PostCardValidationRequest(cardNumber);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("message", out var message));
        Assert.Equal("Card validation successful", message.GetString());
        Assert.True(responseData.TryGetProperty("cardType", out _));
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("12345678901234567890123")] // Too long
    [InlineData("4532015112830367")] // Invalid checksum
    public async Task ValidateCard_WithMultipleInvalidCards_ReturnsError(string cardNumber)
    {
        // Act
        var response = await PostCardValidationRequest(cardNumber);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.RequestUriTooLong);

        var responseContent = await response.Content.ReadAsStringAsync();
        var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

        Assert.True(responseData.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task ValidateCard_WithNullRequestBody_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("null", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/cardvalidation", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateCard_WithInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var content = new StringContent("invalid json", Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/cardvalidation", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ValidateCard_EndpointAccessibility_ReturnsValidResponse()
    {
        // Arrange - Simple test to ensure endpoint is accessible
        var testCard = "4532015112830366";

        // Act
        var response = await PostCardValidationRequest(testCard);

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}