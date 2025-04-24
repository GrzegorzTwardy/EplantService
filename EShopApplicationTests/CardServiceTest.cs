using EShopApplication;

namespace EShopApplicationTests;

public class CardServiceTest
{
    [Fact]
    public void ValidateCard_WithValidCards_ReturnsTrue()
    {
        // Arrange
        var cardService = new CardService();
        var validCards = new[]
        {
            "4532015112830366", // Visa
            "5555555555554444", // MasterCard
            "378282246310005", // American Express
            "6011111111111117", // Discover
            "3530111333300000", // JCB
            "30569309025904", // Diners Club
            "5018000000000009" // Maestro
        };

        // Act & Assert
        foreach (var card in validCards) Assert.True(cardService.ValidateCard(card), $"Card {card} should be valid");
    }

    [Fact]
    public void ValidateCard_WithInvalidChecksum_ReturnsFalse()
    {
        // Arrange
        var cardService = new CardService();
        var invalidCards = new[]
        {
            "4532015112830367", // Visa with wrong checksum
            "5555555555554443", // MasterCard with wrong checksum
            "378282246310006", // American Express with wrong checksum
            "6011111111111118" // Discover with wrong checksum
        };

        // Act & Assert
        foreach (var card in invalidCards)
            Assert.False(cardService.ValidateCard(card), $"Card {card} should be invalid");
    }

    [Fact]
    public void ValidateCard_WithCardContainingSpaces_ProcessesCorrectly()
    {
        // Arrange
        var cardService = new CardService();

        // Act & Assert
        Assert.True(cardService.ValidateCard("4532 0151 1283 0366")); // Valid card with spaces
        Assert.False(cardService.ValidateCard("4532 0151 1283 0367")); // Invalid card with spaces
    }

    [Fact]
    public void ValidateCard_WithNonNumericCharacters_ReturnsFalse()
    {
        // Arrange
        var cardService = new CardService();
        var invalidCards = new[]
        {
            "4532015112830366A", // Contains letter
            "453201511283036X", // Contains letter
            "4532-0151-1283-0366", // Contains dashes (method doesn't remove them)
            "ABCDEFGHIJKLMNOP", // All letters
            "4532O151128303G6" // With letters that look like numbers
        };

        // Act & Assert
        foreach (var card in invalidCards)
            Assert.False(cardService.ValidateCard(card),
                $"Card {card} should be invalid due to non-numeric characters");
    }

    [Theory]
    [InlineData("4532015112830366", "Visa")]
    [InlineData("4111111111111111", "Visa")]
    [InlineData("4012888888881881", "Visa")]
    [InlineData("4222222222222", "Visa")] // 13-digit Visa
    [InlineData("4532015112830366123", "Visa")] // 19-digit Visa
    public void GetCardType_WithVisaCards_ReturnsVisaType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("5555555555554444", "MasterCard")]
    [InlineData("5105105105105100", "MasterCard")]
    public void GetCardType_WithMasterCards_ReturnsMasterCardType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("378282246310005", "American Express")]
    [InlineData("371449635398431", "American Express")]
    [InlineData("340000000000009", "American Express")]
    [InlineData("370000000000002", "American Express")]
    public void GetCardType_WithAmexCards_ReturnsAmexType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("6011111111111117", "Discover")]
    [InlineData("6011000990139424", "Discover")]
    [InlineData("6500000000000002", "Discover")] // 65 prefix
    [InlineData("6445644564456445", "Discover")] // 644-649 prefix
    public void GetCardType_WithDiscoverCards_ReturnsDiscoverType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("3530111333300000", "JCB")]
    [InlineData("3566002020360505", "JCB")]
    [InlineData("3528000000000007", "JCB")]
    [InlineData("3589000000000008", "JCB")]
    public void GetCardType_WithJCBCards_ReturnsJCBType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("30569309025904", "Diners Club")]
    [InlineData("38520000023237", "Diners Club")]
    [InlineData("30000000000004", "Diners Club")]
    [InlineData("36000000000008", "Diners Club")]
    public void GetCardType_WithDinersClubCards_ReturnsDinersClubType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Theory]
    [InlineData("5018000000000009", "Maestro")]
    [InlineData("5020000000000007", "Maestro")]
    [InlineData("5038000000000005", "Maestro")]
    [InlineData("6304000000000008", "Maestro")]
    public void GetCardType_WithMaestroCards_ReturnsMaestroType(string cardNumber, string expectedType)
    {
        // Arrange
        var cardService = new CardService();

        // Act
        var result = cardService.GetCardType(cardNumber);

        // Assert
        Assert.Equal(expectedType, result);
    }

    [Fact]
    public void GetCardType_WithSpacesAndDashes_ProcessesCorrectly()
    {
        // Arrange
        var cardService = new CardService();

        // Act & Assert
        Assert.Equal("Visa", cardService.GetCardType("4532 0151 1283 0366"));
        Assert.Equal("Visa", cardService.GetCardType("4532-0151-1283-0366"));
        Assert.Equal("MasterCard", cardService.GetCardType("5555-5555-5555-4444"));
        Assert.Equal("American Express", cardService.GetCardType("3782 822463 10005"));
    }

    [Fact]
    public void GetCardType_WithUnsupportedCardType_ThrowsArgumentException()
    {
        // Arrange
        var cardService = new CardService();
        var invalidCards = new[]
        {
            "1234567890123456", // Random numbers
            "9999999999999999", // Not a valid card type prefix
            "0000000000000000", // All zeros
            "1111111111111111" // All ones
        };

        // Act & Assert
        foreach (var card in invalidCards)
        {
            var exception = Assert.Throws<ArgumentException>(() => cardService.GetCardType(card));
            Assert.Equal("Unsupported card type (Parameter 'cardNumber')", exception.Message);
            Assert.Equal("cardNumber", exception.ParamName);
        }
    }

    [Fact]
    public void GetCardType_WithInvalidInput_ThrowsException()
    {
        // Arrange
        var cardService = new CardService();

        // Act & Assert
        // Empty string
        Assert.Throws<ArgumentException>(() => cardService.GetCardType(""));

        // Too short
        Assert.Throws<ArgumentException>(() => cardService.GetCardType("1234"));

        // With non-numeric characters (after removing spaces and dashes)
        Assert.Throws<ArgumentException>(() => cardService.GetCardType("4111-1111-1111-111X"));
    }

    [Fact]
    public void ValidateCard_AndGetCardType_CombinedTest()
    {
        // Arrange
        var cardService = new CardService();
        var testCards = new[]
        {
            ("4111111111111111", true, "Visa"),
            ("5555555555554444", true, "MasterCard"),
            ("378282246310005", true, "American Express"),
            ("6011111111111117", true, "Discover"),
            ("4111111111111112", false, "Visa"), // Invalid checksum but valid format
            ("1234567890123456", false, null) // Invalid format and checksum
        };

        // Act & Assert
        foreach (var (cardNumber, isValid, expectedType) in testCards)
        {
            // Check validation
            Assert.Equal(isValid, cardService.ValidateCard(cardNumber));

            // Check type for cards with valid format
            if (expectedType != null)
                Assert.Equal(expectedType, cardService.GetCardType(cardNumber));
            else
                Assert.Throws<ArgumentException>(() => cardService.GetCardType(cardNumber));
        }
    }
}