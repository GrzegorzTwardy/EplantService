using EShopApplication;
using EShopDomain.exceptions;

namespace EShopApplicationTests;

public class CardLengthServiceTest
{
    private readonly CardLengthService _cardService = new();

    [Fact]
    public void ValidateCardLength_WithValidLength_ShouldNotThrowException()
    {
        // Arrange
        var validCardNumbers = new[]
        {
            "4111111111111111", // 16 digits
            "378282246310005", // 15 digits
            "4222222222222", // 13 digits (minimum)
            "4111111111111111111" // 19 digits (maximum)
        };

        // Act & Assert
        foreach (var cardNumber in validCardNumbers)
            // No exception should be thrown
            _cardService.ValidateCardLength(cardNumber);
    }

    [Fact]
    public void ValidateCardLength_WithSpacesAndDashes_ShouldRemoveThemAndValidate()
    {
        // Arrange
        var formattedCardNumbers = new[]
        {
            "4111 1111 1111 1111", // With spaces
            "4111-1111-1111-1111", // With dashes
            "4111 1111-1111 1111", // With both
            " 4111111111111111 " // With leading/trailing spaces
        };

        // Act & Assert
        foreach (var cardNumber in formattedCardNumbers)
            // No exception should be thrown
            _cardService.ValidateCardLength(cardNumber);
    }

    [Fact]
    public void ValidateCardLength_WithEmptyString_ShouldThrowCardNumberInvalidException()
    {
        // Arrange
        var emptyCardNumber = "";

        // Act & Assert
        var exception = Assert.Throws<CardNumberInvalidException>(() =>
            _cardService.ValidateCardLength(emptyCardNumber));

        Assert.Equal("Card number cannot be empty.", exception.Message);
    }

    [Fact]
    public void ValidateCardLength_WithNullValue_ShouldThrowCardNumberInvalidException()
    {
        // Arrange
        string nullCardNumber = null;

        // Act & Assert
        var exception = Assert.Throws<CardNumberInvalidException>(() =>
            _cardService.ValidateCardLength(nullCardNumber));

        Assert.Equal("Card number cannot be empty.", exception.Message);
    }

    [Fact]
    public void ValidateCardLength_WithWhitespaceOnly_ShouldThrowCardNumberInvalidException()
    {
        // Arrange
        var whitespaceCardNumber = "   ";

        // Act & Assert
        var exception = Assert.Throws<CardNumberInvalidException>(() =>
            _cardService.ValidateCardLength(whitespaceCardNumber));

        Assert.Equal("Card number cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("123456789012")] // 12 digits (one less than minimum)
    public void ValidateCardLength_WithTooShortNumber_ShouldThrowCardNumberTooShortException(string cardNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<CardNumberTooShortException>(() =>
            _cardService.ValidateCardLength(cardNumber));

        Assert.Contains($"must be at least {13} digits", exception.Message);
        Assert.Equal(13, exception.MinRequiredLength);
        Assert.Equal(cardNumber.Length, exception.ActualLength);
    }

    [Theory]
    [InlineData("12345678901234567890")] // 20 digits (one more than maximum)
    [InlineData("123456789012345678901234567890")] // 30 digits
    public void ValidateCardLength_WithTooLongNumber_ShouldThrowCardNumberTooLongException(string cardNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<CardNumberTooLongException>(() =>
            _cardService.ValidateCardLength(cardNumber));

        Assert.Contains($"cannot exceed {19} digits", exception.Message);
        Assert.Equal(19, exception.MaxAllowedLength);
        Assert.Equal(cardNumber.Length, exception.ActualLength);
    }

    [Theory]
    [InlineData("4111111111111111A")] // Letter at end
    [InlineData("41111111A1111111")] // Letter in middle
    [InlineData("A4111111111111111")] // Letter at beginning
    [InlineData("4111 1111 1111 111X")] // With spaces and letter
    [InlineData("4111@1111$1111*1111")] // With special characters
    public void ValidateCardLength_WithNonNumericCharacters_ShouldThrowCardNumberInvalidException(string cardNumber)
    {
        // Act & Assert
        var exception = Assert.Throws<CardNumberInvalidException>(() =>
            _cardService.ValidateCardLength(cardNumber));

        Assert.Contains("can only contain digits", exception.Message);
    }

    [Fact]
    public void ValidateCardLength_WithOnlyDashesAndSpaces_ShouldThrowCardNumberInvalidException()
    {
        // Arrange
        var spacesAndDashes = "-- -- --";

        // Act & Assert
        var exception = Assert.Throws<CardNumberInvalidException>(() =>
            _cardService.ValidateCardLength(spacesAndDashes));

        Assert.Equal("Card number cannot be empty.", exception.Message);
    }
}