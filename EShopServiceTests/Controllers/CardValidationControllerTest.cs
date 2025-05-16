using EShopApplication;
using EShopDomain.exceptions;
using EShopService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EShopServiceTests.Controllers;

public class CardValidationControllerTest
{
    private readonly CardValidationController _controller;
    private readonly Mock<ICardLengthService> _mockCardLengthService;
    private readonly Mock<ICardService> _mockCardService;

    public CardValidationControllerTest()
    {
        _mockCardLengthService = new Mock<ICardLengthService>();
        _mockCardService = new Mock<ICardService>();
        _controller = new CardValidationController(_mockCardLengthService.Object, _mockCardService.Object);
    }

    [Fact]
    public void ValidateCard_WithValidVisaCard_ReturnsOkResult()
    {
        // Arrange
        var cardNumber = "4532015112830366";
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(true);
        _mockCardService.Setup(x => x.GetCardType(cardNumber)).Returns("Visa");

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithValidMasterCardCard_ReturnsOkResult()
    {
        // Arrange
        var cardNumber = "5555555555554444";
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(true);
        _mockCardService.Setup(x => x.GetCardType(cardNumber)).Returns("MasterCard");

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithInvalidChecksum_ReturnsBadRequest()
    {
        // Arrange
        var cardNumber = "4532015112830367";
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(false);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithUnsupportedCardType_ReturnsNotAcceptable()
    {
        // Arrange
        var cardNumber = "6011000990139424";
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(true);
        _mockCardService.Setup(x => x.GetCardType(cardNumber)).Returns("Discover");

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.Equal(406, objectResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithTooShortCardNumber_ReturnsBadRequest()
    {
        // Arrange
        var cardNumber = "123456789012";
        var exception = new CardNumberTooShortException("Card number is too short.", 13, 12);
        _mockCardLengthService.Setup(x => x.ValidateCardLength(cardNumber)).Throws(exception);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithTooLongCardNumber_ReturnsUriTooLong()
    {
        // Arrange
        var cardNumber = "12345678901234567890";
        var exception = new CardNumberTooLongException("Card number is too long.", 19, 20);
        _mockCardLengthService.Setup(x => x.ValidateCardLength(cardNumber)).Throws(exception);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.Equal(414, objectResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithInvalidCardNumber_ReturnsBadRequest()
    {
        // Arrange
        var cardNumber = "4532abc112830366";
        var exception = new CardNumberInvalidException("Card number contains invalid characters.");
        _mockCardLengthService.Setup(x => x.ValidateCardLength(cardNumber)).Throws(exception);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public void ValidateCard_WithEmptyCardNumber_ReturnsBadRequest()
    {
        // Arrange
        var cardNumber = "";
        var exception = new CardNumberInvalidException("Card number cannot be empty.");
        _mockCardLengthService.Setup(x => x.ValidateCardLength(cardNumber)).Throws(exception);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Theory]
    [InlineData("4532015112830366", "Visa")]
    [InlineData("5555555555554444", "MasterCard")]
    [InlineData("378282246310005", "American Express")]
    public void ValidateCard_WithValidCards_ReturnsOk(string cardNumber, string cardType)
    {
        // Arrange
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(true);
        _mockCardService.Setup(x => x.GetCardType(cardNumber)).Returns(cardType);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Theory]
    [InlineData("JCB")]
    [InlineData("Diners Club")]
    [InlineData("Maestro")]
    [InlineData("Discover")]
    public void ValidateCard_WithUnsupportedCardTypes_Returns406(string cardType)
    {
        // Arrange
        var cardNumber = "4532015112830366";
        _mockCardService.Setup(x => x.ValidateCard(cardNumber)).Returns(true);
        _mockCardService.Setup(x => x.GetCardType(cardNumber)).Returns(cardType);

        // Act
        var result = _controller.ValidateCard(cardNumber);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.Equal(406, objectResult.StatusCode);
    }
}