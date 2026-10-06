using Coforge_Task.Models;

namespace Coforge_Task.Services;

/// <summary>
/// Interface for external payment service.
/// Assumption: This service is responsible for charging customers.
/// </summary>
public interface IPaymentService
{
    Task<PaymentResult> ChargeAsync(decimal amount);
}

/// <summary>
/// Interface for external gift card provider.
/// Assumption: This service generates and provides gift card codes.
/// </summary>
public interface IGiftCardProvider
{
    Task<GiftCardResult> GenerateAsync(string giftCardType, decimal amount);
}
