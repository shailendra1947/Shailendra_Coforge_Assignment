using Coforge_Task.Models;

namespace Coforge_Task.Services.Implementations;

/// <summary>
/// Mock implementation of the payment service.
/// Assumption: This simulates a real payment processor that charges customers.
/// Current behavior: 95% success rate. Fails if amount is exactly 0 or negative.
/// </summary>
public class MockPaymentService : IPaymentService
{
    private static int _callCount = 0;

    public Task<PaymentResult> ChargeAsync(decimal amount)
    {
        // Simulate occasional payment failures
        _callCount++;
        bool shouldFail = _callCount % 20 == 0; // ~5% failure rate

        if (amount <= 0)
        {
            return Task.FromResult(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Invalid amount: must be greater than 0",
                Amount = amount
            });
        }

        var result = new PaymentResult
        {
            Success = !shouldFail,
            TransactionId = shouldFail ? string.Empty : $"TXN-{Guid.NewGuid():N}",
            ErrorMessage = shouldFail ? "Payment gateway timeout" : null,
            Amount = amount
        };

        return Task.FromResult(result);
    }
}
