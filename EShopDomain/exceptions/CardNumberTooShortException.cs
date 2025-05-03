namespace EShopDomain.exceptions;

public class CardNumberTooShortException : Exception
{
    public CardNumberTooShortException()
        : base("The card number is too short.")
    {
    }

    public CardNumberTooShortException(string message)
        : base(message)
    {
    }

    public CardNumberTooShortException(string message, int minRequiredLength, int actualLength)
        : base(message)
    {
        MinRequiredLength = minRequiredLength;
        ActualLength = actualLength;
    }

    public CardNumberTooShortException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public int MinRequiredLength { get; }
    public int ActualLength { get; }
}