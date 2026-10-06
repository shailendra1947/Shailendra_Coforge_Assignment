namespace Coforge_Task.DTOs;

/// <summary>
/// Request to purchase a gift card.
/// </summary>
public class PurchaseGiftCardRequest
{
    /// <summary>
    /// The unique customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// The type of gift card to purchase (Amazon, Apple, GooglePlay, Microsoft).
    /// </summary>
    public string GiftCardType { get; set; } = string.Empty;

    /// <summary>
    /// The amount to charge for the gift card.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
/// Response after purchasing a gift card.
/// </summary>
public class PurchaseGiftCardResponse
{
    /// <summary>
    /// The unique order identifier.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// The order status (Pending, Completed, Failed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The generated gift card code (only present if purchase succeeded).
    /// </summary>
    public string? GiftCardCode { get; set; }

    /// <summary>
    /// Error message (only present if purchase failed).
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Response when retrieving an existing order.
/// </summary>
public class OrderResponse
{
    /// <summary>
    /// The unique order identifier.
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// The customer identifier.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// The order amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// The order status (Pending, Completed, Failed).
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The gift card code associated with this order.
    /// </summary>
    public string? GiftCardCode { get; set; }
}

/// <summary>
/// Error response.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
