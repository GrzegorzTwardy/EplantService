namespace EShopDomain.exceptions;

public class CardNumberTooLongException : Exception
{
    public CardNumberTooLongException()
        : base("The card number is too long.")
    {
    }

    public CardNumberTooLongException(string message)
        : base(message)
    {
    }

    public CardNumberTooLongException(string message, int maxAllowedLength, int actualLength)
        : base(message)
    {
        MaxAllowedLength = maxAllowedLength;
        ActualLength = actualLength;
    }

    public CardNumberTooLongException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public int MaxAllowedLength { get; }
    public int ActualLength { get; }
}