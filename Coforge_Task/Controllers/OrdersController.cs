using Coforge_Task.DTOs;
using Coforge_Task.Services;
using Microsoft.AspNetCore.Mvc;

namespace Coforge_Task.Controllers;

/// <summary>
/// API controller for managing orders.
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IGiftCardPurchaseService _giftCardService;

    /// <summary>
    /// Initializes a new instance of the OrdersController class.
    /// </summary>
    public OrdersController(IGiftCardPurchaseService giftCardService)
    {
        _giftCardService = giftCardService;
    }

    /// <summary>
    /// Get an existing order by ID.
    /// </summary>
    /// <param name="id">The order ID.</param>
    /// <returns>Order details including customer info, amount, and status.</returns>
    /// <response code="200">Order found and returned.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder(string id)
    {
        var order = await _giftCardService.GetOrderAsync(id);

        if (order == null)
            return NotFound(new ErrorResponse { Message = $"Order {id} not found" });

        return Ok(order);
    }
}
