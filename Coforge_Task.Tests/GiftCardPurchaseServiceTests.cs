using Coforge_Task.Data;
using Coforge_Task.DTOs;
using Coforge_Task.Models;
using Coforge_Task.Services;
using Coforge_Task.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Coforge_Task.Tests;

/// <summary>
/// Unit tests for GiftCardPurchaseService.
/// These tests cover all specified scenarios including success, failures, and edge cases.
/// Assumption: In-memory database is used for testing to avoid external dependencies.
/// </summary>
public class GiftCardPurchaseServiceTests
{
    private GiftCardContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<GiftCardContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new GiftCardContext(options);
    }

    [Fact]
    public async Task PurchaseAsync_WithValidRequest_ReturnsCompletedOrder()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST001",
            GiftCardType = "Amazon",
            Amount = 100
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Completed", response.Status);
        Assert.False(string.IsNullOrEmpty(response.OrderId));
        Assert.False(string.IsNullOrEmpty(response.GiftCardCode));

        // Verify order was saved
        var order = await context.Orders.FirstOrDefaultAsync(o => o.OrderId == response.OrderId);
        Assert.NotNull(order);
        Assert.Equal("Completed", order.Status);
        Assert.Equal("CUST001", order.CustomerId);
        Assert.Equal(100, order.Amount);
    }

    [Fact]
    public async Task PurchaseAsync_WithInvalidGiftCardType_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST001",
            GiftCardType = "InvalidType",
            Amount = 100
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("Invalid gift card type", response.ErrorMessage);
        Assert.True(string.IsNullOrEmpty(response.OrderId));
    }

    [Fact]
    public async Task PurchaseAsync_WithPaymentFailure_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentServiceMock = new Mock<IPaymentService>();
        paymentServiceMock
            .Setup(p => p.ChargeAsync(It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = false,
                ErrorMessage = "Payment gateway declined"
            });

        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentServiceMock.Object, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST002",
            GiftCardType = "Amazon",
            Amount = 100
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("Payment failed", response.ErrorMessage);

        // Verify order was created but marked as failed
        var order = await context.Orders.FirstOrDefaultAsync();
        Assert.NotNull(order);
        Assert.Equal("Failed", order.Status);
    }

    [Fact]
    public async Task PurchaseAsync_WithGiftCardProviderFailure_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProviderMock = new Mock<IGiftCardProvider>();
        giftCardProviderMock
            .Setup(g => g.GenerateAsync(It.IsAny<string>(), It.IsAny<decimal>()))
            .ReturnsAsync(new GiftCardResult
            {
                Success = false,
                ErrorMessage = "Provider service unavailable"
            });

        var service = new GiftCardPurchaseService(context, paymentService, giftCardProviderMock.Object);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST003",
            GiftCardType = "Amazon",
            Amount = 100
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("Gift card generation failed", response.ErrorMessage);

        // Verify order was created but marked as failed
        var order = await context.Orders.FirstOrDefaultAsync();
        Assert.NotNull(order);
        Assert.Equal("Failed", order.Status);
    }

    [Fact]
    public async Task GetOrderAsync_WithExistingOrder_ReturnsOrder()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST004",
            GiftCardType = "Apple",
            Amount = 50
        };

        var purchaseResponse = await service.PurchaseAsync(request);

        // Act
        var getResponse = await service.GetOrderAsync(purchaseResponse.OrderId);

        // Assert
        Assert.NotNull(getResponse);
        Assert.Equal(purchaseResponse.OrderId, getResponse.OrderId);
        Assert.Equal("CUST004", getResponse.CustomerId);
        Assert.Equal(50, getResponse.Amount);
        Assert.Equal("Completed", getResponse.Status);
        Assert.NotNull(getResponse.GiftCardCode);
    }

    [Fact]
    public async Task GetOrderAsync_WithNonExistentOrder_ReturnsNull()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        // Act
        var response = await service.GetOrderAsync("NON-EXISTENT");

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task PurchaseAsync_WithDuplicateRequest_ReturnsPreviousOrder()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST005",
            GiftCardType = "GooglePlay",
            Amount = 75
        };

        // First purchase
        var firstResponse = await service.PurchaseAsync(request);

        // Act - Second purchase with same details
        var secondResponse = await service.PurchaseAsync(request);

        // Assert - Should return same order (idempotency)
        Assert.Equal(firstResponse.OrderId, secondResponse.OrderId);
        Assert.Equal("Completed", secondResponse.Status);
        Assert.Equal(firstResponse.GiftCardCode, secondResponse.GiftCardCode);
    }

    [Fact]
    public async Task PurchaseAsync_WithMissingCustomerId_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "",
            GiftCardType = "Amazon",
            Amount = 100
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("CustomerId is required", response.ErrorMessage);
    }

    [Fact]
    public async Task PurchaseAsync_WithZeroAmount_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST006",
            GiftCardType = "Amazon",
            Amount = 0
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("Amount must be greater than 0", response.ErrorMessage);
    }

    [Fact]
    public async Task PurchaseAsync_WithNegativeAmount_ReturnsFailed()
    {
        // Arrange
        var context = CreateInMemoryContext();
        var paymentService = new MockPaymentService();
        var giftCardProvider = new MockGiftCardProvider();
        var service = new GiftCardPurchaseService(context, paymentService, giftCardProvider);

        var request = new PurchaseGiftCardRequest
        {
            CustomerId = "CUST007",
            GiftCardType = "Microsoft",
            Amount = -50
        };

        // Act
        var response = await service.PurchaseAsync(request);

        // Assert
        Assert.Equal("Failed", response.Status);
        Assert.Contains("Amount must be greater than 0", response.ErrorMessage);
    }

    [Fact]
    public async Task GiftCardTypesValidation_AllSupportedTypes_AreValid()
    {
        // Arrange & Act & Assert
        Assert.True(GiftCardTypes.IsValid("Amazon"));
        Assert.True(GiftCardTypes.IsValid("Apple"));
        Assert.True(GiftCardTypes.IsValid("GooglePlay"));
        Assert.True(GiftCardTypes.IsValid("Microsoft"));
        Assert.False(GiftCardTypes.IsValid("InvalidType"));
        Assert.False(GiftCardTypes.IsValid(""));
        Assert.False(GiftCardTypes.IsValid(null));
    }
}
