namespace EShopDomain.exceptions;

public class CardNumberInvalidException : Exception
{
    public CardNumberInvalidException()
        : base("The card number is invalid.")
    {
    }

    public CardNumberInvalidException(string message)
        : base(message)
    {
    }

    public CardNumberInvalidException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}