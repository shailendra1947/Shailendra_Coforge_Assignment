using Coforge_Task.Models;

namespace Coforge_Task.Services.Implementations;

/// <summary>
/// Mock implementation of the gift card provider.
/// Assumption: This simulates an external system that generates unique gift card codes.
/// Current behavior: Generates codes based on gift card type and amount.
/// Validates that the gift card type is supported.
/// </summary>
public class MockGiftCardProvider : IGiftCardProvider
{
    public Task<GiftCardResult> GenerateAsync(string giftCardType, decimal amount)
    {
        // Validate gift card type
        if (!GiftCardTypes.IsValid(giftCardType))
        {
            return Task.FromResult(new GiftCardResult
            {
                Success = false,
                ErrorMessage = $"Unsupported gift card type: {giftCardType}"
            });
        }

        // Validate amount
        if (amount <= 0)
        {
            return Task.FromResult(new GiftCardResult
            {
                Success = false,
                ErrorMessage = "Amount must be greater than 0"
            });
        }

        // Generate a mock gift card code
        var prefix = giftCardType switch
        {
            GiftCardTypes.Amazon => "AMZ",
            GiftCardTypes.AppleGiftCard => "APPL",
            GiftCardTypes.GooglePlay => "GOOG",
            GiftCardTypes.MicrosoftStore => "MSFT",
            _ => "UNKN"
        };

        var giftCardCode = $"{prefix}-{Guid.NewGuid():N}".Substring(0, 25);

        var result = new GiftCardResult
        {
            Success = true,
            GiftCardCode = giftCardCode
        };

        return Task.FromResult(result);
    }
}
