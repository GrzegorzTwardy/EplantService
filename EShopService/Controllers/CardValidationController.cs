using EShopApplication;
using EShopDomain.enums;
using EShopDomain.exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EShopService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CardValidationController : ControllerBase
{
    private readonly ICardLengthService _cardLengthService;
    private readonly ICardService _cardService;

    // services injected by framework
    public CardValidationController(ICardLengthService cardLengthService, ICardService cardService)
    {
        _cardLengthService = cardLengthService;
        _cardService = cardService;
    }

    /// <summary>
    ///     Validates a credit card number
    /// </summary>
    /// <param name="cardNumber">The credit card number to validate</param>
    /// <returns>
    ///     200 OK - If card is valid
    ///     400 Bad Request - If card is too short or invalid format
    ///     406 Not Acceptable - If card type is not supported
    ///     414 URI Too Long - If card number is too long
    /// </returns>
    [HttpPost]
    public IActionResult ValidateCard([FromBody] string cardNumber)
    {
        try
        {
            // Step 1: Validate card length
            _cardLengthService.ValidateCardLength(cardNumber);

            // Step 2: Validate card using Luhn algorithm
            if (!_cardService.ValidateCard(cardNumber))
                return BadRequest(new { error = "Card number failed validation check (invalid checksum)." });

            // Step 3: Get card type and verify it's supported in the CreditCardProvider enum
            var cardType = _cardService.GetCardType(cardNumber);

            try
            {
                // Attempt to convert the card type to the CreditCardProvider enum
                var provider = CreditCardProviderExtensions.FromString(cardType);

                // If we reach here, the card type is supported
                return Ok(new { message = "Card validation successful", cardType = provider.ToString() });
            }
            catch (ArgumentException)
            {
                // Card type not supported in our CreditCardProvider enum
                return StatusCode(406, new { error = $"Card type '{cardType}' is not supported." });
            }
        }
        catch (CardNumberTooLongException ex)
        {
            // Return 414 for too long card numbers
            return StatusCode(414, new
            {
                error = ex.Message,
                maxLength = ex.MaxAllowedLength,
                actualLength = ex.ActualLength
            });
        }
        catch (CardNumberTooShortException ex)
        {
            // Return 400 for too short card numbers
            return BadRequest(new
            {
                error = ex.Message,
                minLength = ex.MinRequiredLength,
                actualLength = ex.ActualLength
            });
        }
        catch (CardNumberInvalidException ex)
        {
            // Return 400 for other validation errors
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            // Return 400 for other argument exceptions (like unsupported card type)
            return BadRequest(new { error = ex.Message });
        }
    }
}