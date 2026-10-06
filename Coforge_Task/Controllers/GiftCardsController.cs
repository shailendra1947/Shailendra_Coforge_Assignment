using Coforge_Task.DTOs;
using Coforge_Task.Services;
using Microsoft.AspNetCore.Mvc;

namespace Coforge_Task.Controllers;

/// <summary>
/// API controller for managing gift card purchases.
/// </summary>
[ApiController]
[Route("api/giftcards")]
public class GiftCardsController : ControllerBase
{
    private readonly IGiftCardPurchaseService _giftCardService;

    /// <summary>
    /// Initializes a new instance of the GiftCardsController class.
    /// </summary>
    public GiftCardsController(IGiftCardPurchaseService giftCardService)
    {
        _giftCardService = giftCardService;
    }

    /// <summary>
    /// Purchase a digital gift card.
    /// </summary>
    /// <param name="request">The purchase request containing customer ID, gift card type, and amount.</param>
    /// <returns>Order details with gift card code if successful.</returns>
    /// <response code="200">Gift card purchased successfully.</response>
    /// <response code="400">Invalid request or payment/provider failure.</response>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(PurchaseGiftCardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PurchaseGiftCardResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PurchaseGiftCardResponse>> Purchase([FromBody] PurchaseGiftCardRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _giftCardService.PurchaseAsync(request);

        if (response.Status == "Failed")
            return BadRequest(response);

        return Ok(response);
    }
}
