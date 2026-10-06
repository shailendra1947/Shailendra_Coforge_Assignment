namespace Coforge_Task.Models;

/// <summary>
/// Result from the payment service after charging a customer.
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Result from the gift card provider after generating a gift card code.
/// </summary>
public class GiftCardResult
{
    public bool Success { get; set; }
    public string GiftCardCode { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}
