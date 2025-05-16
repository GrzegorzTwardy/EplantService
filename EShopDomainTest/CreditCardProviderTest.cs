using EShopDomain.enums;

namespace EShopDomainTest;

public class CreditCardProviderExtensionsTests
{
    [Theory]
    [InlineData("Visa", CreditCardProvider.Visa)]
    [InlineData("visa", CreditCardProvider.Visa)]
    [InlineData("VISA", CreditCardProvider.Visa)]
    [InlineData("ViSa", CreditCardProvider.Visa)]
    public void FromString_WhenVisaVariations_ReturnsVisaEnum(string input, CreditCardProvider expected)
    {
        var result = CreditCardProviderExtensions.FromString(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Mastercard", CreditCardProvider.MasterCard)]
    [InlineData("mastercard", CreditCardProvider.MasterCard)]
    [InlineData("MASTERCARD", CreditCardProvider.MasterCard)]
    [InlineData("MaStErCaRd", CreditCardProvider.MasterCard)]
    public void FromString_WhenMastercardVariations_ReturnsMastercardEnum(string input, CreditCardProvider expected)
    {
        var result = CreditCardProviderExtensions.FromString(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("AmericanExpress", CreditCardProvider.AmericanExpress)]
    [InlineData("americanexpress", CreditCardProvider.AmericanExpress)]
    [InlineData("AMERICANEXPRESS", CreditCardProvider.AmericanExpress)]
    [InlineData("American Express", CreditCardProvider.AmericanExpress)]
    [InlineData("american express", CreditCardProvider.AmericanExpress)]
    [InlineData("AMERICAN EXPRESS", CreditCardProvider.AmericanExpress)]
    [InlineData("American express", CreditCardProvider.AmericanExpress)]
    public void FromString_WhenAmericanExpressVariations_ReturnsAmericanExpressEnum(string input,
        CreditCardProvider expected)
    {
        var result = CreditCardProviderExtensions.FromString(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void FromString_WhenNullOrWhitespace_ThrowsArgumentException(string input)
    {
        var exception = Assert.Throws<ArgumentException>(() => CreditCardProviderExtensions.FromString(input));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains("Value cannot be null or whitespace", exception.Message);
    }

    [Theory]
    [InlineData("Discover")]
    [InlineData("JCB")]
    [InlineData("UnionPay")]
    [InlineData("Diners Club")]
    [InlineData("Random Text")]
    [InlineData("123456")]
    public void FromString_WhenInvalidProvider_ThrowsArgumentException(string input)
    {
        var exception = Assert.Throws<ArgumentException>(() => CreditCardProviderExtensions.FromString(input));
        Assert.Equal("value", exception.ParamName);
        Assert.Contains($"'{input}' is not a valid credit card provider", exception.Message);
    }

    [Fact]
    public void FromString_WhenTyposInValidProviders_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreditCardProviderExtensions.FromString("Visaa"));
        Assert.Throws<ArgumentException>(() => CreditCardProviderExtensions.FromString("Master card"));
        Assert.Throws<ArgumentException>(() => CreditCardProviderExtensions.FromString("American-Express"));
    }
}