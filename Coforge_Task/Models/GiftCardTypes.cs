namespace Coforge_Task.Models;

/// <summary>
/// Supported gift card types.
/// </summary>
public static class GiftCardTypes
{
    public const string Amazon = "Amazon";
    public const string AppleGiftCard = "Apple";
    public const string GooglePlay = "GooglePlay";
    public const string MicrosoftStore = "Microsoft";

    public static readonly HashSet<string> ValidTypes = new()
    {
        Amazon,
        AppleGiftCard,
        GooglePlay,
        MicrosoftStore
    };

    public static bool IsValid(string giftCardType)
    {
        return !string.IsNullOrWhiteSpace(giftCardType) && ValidTypes.Contains(giftCardType);
    }
}
