﻿using System.Text.RegularExpressions;

namespace EShopApplication;

public interface ICardService
{
    bool ValidateCard(string cardNumber);
    string GetCardType(string cardNumber);
}

public class CardService : ICardService
{
    public bool ValidateCard(string cardNumber)
    {
        cardNumber = cardNumber.Replace(" ", "");
        if (!cardNumber.All(char.IsDigit))
            return false;

        var sum = 0;
        var alternate = false;

        for (var i = cardNumber.Length - 1; i >= 0; i--)
        {
            var digit = cardNumber[i] - '0';

            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    public string GetCardType(string cardNumber)
    {
        cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

        if (Regex.IsMatch(cardNumber, @"^4(\d{12}|\d{15}|\d{18})$"))
            return "Visa";
        if (Regex.IsMatch(cardNumber,
                @"^(5[1-5]\d{14}|2(2[2-9][1-9]|2[3-9]\d{2}|[3-6]\d{3}|7([01]\d{2}|20\d))\d{10})$"))
            return "MasterCard";

        if (Regex.IsMatch(cardNumber, @"^3[47]\d{13}$"))
            return "American Express";

        if (Regex.IsMatch(cardNumber,
                @"^(6011\d{12}|65\d{14}|64[4-9]\d{13}|622(1[2-9][6-9]|[2-8]\d{2}|9([01]\d|2[0-5]))\d{10})$"))
            return "Discover";

        if (Regex.IsMatch(cardNumber, @"^(352[89]|35[3-8]\d)\d{12}$"))
            return "JCB";

        if (Regex.IsMatch(cardNumber, @"^3(0[0-5]|[68]\d)\d{11}$"))
            return "Diners Club";

        if (Regex.IsMatch(cardNumber, @"^(50|5[6-9]|6\d)\d{10,17}$"))
            return "Maestro";

        throw new ArgumentException("Unsupported card type", nameof(cardNumber));
    }
}