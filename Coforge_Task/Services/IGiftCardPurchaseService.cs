using Coforge_Task.DTOs;

namespace Coforge_Task.Services
{
    public interface IGiftCardPurchaseService
    {
        Task<PurchaseGiftCardResponse> PurchaseAsync(PurchaseGiftCardRequest request);
        Task<OrderResponse?> GetOrderAsync(string orderId);
    }
}
