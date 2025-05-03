using EShopDomain.exceptions;

namespace EShopApplication;

public class CardLengthService
{
    private const int MinLength = 13;
    private const int MaxLength = 19;

    public void ValidateCardLength(string cardNumber)
    {
        if (string.IsNullOrEmpty(cardNumber)) throw new CardNumberInvalidException("Card number cannot be empty.");

        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (string.IsNullOrEmpty(cardNumber)) throw new CardNumberInvalidException("Card number cannot be empty.");

        if (cardNumber.Length < MinLength)
            throw new CardNumberTooShortException(
                $"Card number must be at least {MinLength} digits. Received {cardNumber.Length} digits.",
                MinLength,
                cardNumber.Length);

        if (cardNumber.Length > MaxLength)
            throw new CardNumberTooLongException(
                $"Card number cannot exceed {MaxLength} digits. Received {cardNumber.Length} digits.",
                MaxLength,
                cardNumber.Length);

        if (!cardNumber.All(char.IsDigit))
            throw new CardNumberInvalidException(
                "Card number can only contain digits. Non-numeric characters detected");
    }
}