namespace EShopDomain.enums;

public enum CreditCardProvider
{
    Visa,
    Mastercard,
    AmericanExpress
}

public static class CreditCardProviderExtensions
{
    public static CreditCardProvider FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        // this additional check is required because TryParse method below may accept numbers
        // if it matches the underlying numeric value of an enum member
        if (value.All(char.IsDigit))
            throw new ArgumentException($"'{value}' is not a valid credit card provider.", nameof(value));

        if (Enum.TryParse(value, true, out CreditCardProvider result))
            return result;

        if (value.Equals("American Express", StringComparison.OrdinalIgnoreCase))
            return CreditCardProvider.AmericanExpress;

        throw new ArgumentException($"'{value}' is not a valid credit card provider.", nameof(value));
    }
}