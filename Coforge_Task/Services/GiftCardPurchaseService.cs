using Coforge_Task.Data;
using Coforge_Task.DTOs;
using Coforge_Task.Models;
using Microsoft.EntityFrameworkCore;

namespace Coforge_Task.Services;

/// <summary>
/// Core service for orchestrating gift card purchases.
/// Assumption: This service coordinates validation, payment, and gift card code generation.
/// Workflow: Validate -> Create Order -> Charge Payment -> Generate Code -> Update Order
/// </summary>
public class GiftCardPurchaseService : IGiftCardPurchaseService
{
    private readonly GiftCardContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IGiftCardProvider _giftCardProvider;

    public GiftCardPurchaseService(GiftCardContext context,IPaymentService paymentService,IGiftCardProvider giftCardProvider)
    {
        _context = context;
        _paymentService = paymentService;
        _giftCardProvider = giftCardProvider;
    }

    /// <summary>
    /// Processes a gift card purchase request.
    /// Returns the order status and gift card code if successful.
    /// </summary>
    public async Task<PurchaseGiftCardResponse> PurchaseAsync(PurchaseGiftCardRequest request)
    {
        // Step 1: Validate input
        var validationError = ValidatePurchaseRequest(request);
        if (!string.IsNullOrEmpty(validationError))
        {
            return new PurchaseGiftCardResponse 
            { 
                Status = "Failed", 
                ErrorMessage = validationError 
            };
        }

        // Step 2: Check for duplicate purchase (idempotency)
        var existingOrder = await _context.Orders
            .FirstOrDefaultAsync(o => 
                o.CustomerId == request.CustomerId && 
                o.GiftCardType == request.GiftCardType &&
                o.Amount == request.Amount &&
                (o.Status == "Pending" || o.Status == "Completed"));

        if (existingOrder != null)
        {
            return new PurchaseGiftCardResponse
            {
                OrderId = existingOrder.OrderId,
                Status = existingOrder.Status,
                GiftCardCode = existingOrder.GiftCardCode
            };
        }

        // Step 3: Create order in Pending status
        var order = new Order
        {
            OrderId = GenerateOrderId(),
            CustomerId = request.CustomerId,
            GiftCardType = request.GiftCardType,
            Amount = request.Amount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Step 4: Charge payment
        var paymentResult = await _paymentService.ChargeAsync(request.Amount);
        if (!paymentResult.Success)
        {
            order.Status = "Failed";
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return new PurchaseGiftCardResponse
            {
                OrderId = order.OrderId,
                Status = "Failed",
                ErrorMessage = $"Payment failed: {paymentResult.ErrorMessage}"
            };
        }

        // Step 5: Generate gift card code
        var giftCardResult = await _giftCardProvider.GenerateAsync(request.GiftCardType, request.Amount);
        if (!giftCardResult.Success)
        {
            order.Status = "Failed";
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return new PurchaseGiftCardResponse
            {
                OrderId = order.OrderId,
                Status = "Failed",
                ErrorMessage = $"Gift card generation failed: {giftCardResult.ErrorMessage}"
            };
        }

        // Step 6: Update order with success
        order.Status = "Completed";
        order.GiftCardCode = giftCardResult.GiftCardCode;
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return new PurchaseGiftCardResponse
        {
            OrderId = order.OrderId,
            Status = "Completed",
            GiftCardCode = giftCardResult.GiftCardCode
        };
    }

    /// <summary>
    /// Retrieves an existing order by its ID.
    /// </summary>
    public async Task<OrderResponse?> GetOrderAsync(string orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            return null;

        return new OrderResponse
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Amount = order.Amount,
            Status = order.Status,
            GiftCardCode = order.GiftCardCode
        };
    }

    private string? ValidatePurchaseRequest(PurchaseGiftCardRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerId))
            return "CustomerId is required";

        if (string.IsNullOrWhiteSpace(request.GiftCardType))
            return "GiftCardType is required";

        if (!GiftCardTypes.IsValid(request.GiftCardType))
            return $"Invalid gift card type: {request.GiftCardType}";

        if (request.Amount <= 0)
            return "Amount must be greater than 0";

        return null;
    }

    private string GenerateOrderId()
    {
        return $"ORD-{Guid.NewGuid():N}".Substring(0, 20);
    }
}
