using System;
using System.Linq;
using System.Threading.Tasks;
using dotnet_store.Models;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace dotnet_store.Services;

public class IyzipayPaymentService : IPaymentService
{
    private readonly DataContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IyzipayPaymentService> _logger;

    public IyzipayPaymentService(DataContext dbContext, IConfiguration configuration, ILogger<IyzipayPaymentService> logger)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string? ErrorMessage, PaymentInfo? Info)> ChargeOrderAsync(
        Order order,
        AppUser user,
        string cardHolderName,
        string cardNumber,
        string expireMonth,
        string expireYear,
        string cvc,
        string? remoteIpAddress)
    {
        try
        {
            var settings = _configuration.GetSection("Iyzipay");
            var options = new Options
            {
                ApiKey = settings["ApiKey"],
                SecretKey = settings["SecretKey"],
                BaseUrl = settings["BaseUrl"] ?? "https://sandbox-api.iyzipay.com"
            };

            var orderItems = await _dbContext.OrderItems
                .Include(oi => oi.Urun)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            var basketItems = orderItems.Select(item => new Iyzipay.Model.BasketItem
            {
                Id = item.UrunId.ToString(),
                Name = item.Urun?.ProductName ?? ($"Ürün {item.UrunId}"),
                Category1 = item.Urun?.TopMostCategoryNames ?? "Genel",
                ItemType = BasketItemType.PHYSICAL.ToString(),
                Price = (item.LineTotal).ToString(System.Globalization.CultureInfo.InvariantCulture)
            }).ToList();

            if (order.ShippingCost > 0)
            {
                basketItems.Add(new Iyzipay.Model.BasketItem
                {
                    Id = "SHIP",
                    Name = $"Kargo ({order.ShippingProvider})",
                    Category1 = "Kargo",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = order.ShippingCost.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
            }

            var buyer = new Buyer
            {
                Id = user.Id.ToString(),
                Name = (user.AdSoyad ?? user.UserName ?? "").Split(' ').FirstOrDefault() ?? "Müşteri",
                Surname = (user.AdSoyad ?? user.UserName ?? "").Split(' ').Skip(1).DefaultIfEmpty("").Aggregate((a,b) => ($"{a} {b}")).Trim(),
                GsmNumber = "+900000000000",
                Email = user.Email ?? "email@email.com",
                IdentityNumber = "11111111110",
                LastLoginDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationAddress = "",
                Ip = remoteIpAddress ?? "127.0.0.1",
                City = "",
                Country = "Turkey",
                ZipCode = "00000"
            };

            Address shippingAddress = new Address();
            Address billingAddress = new Address();
            if (order.AddressId.HasValue)
            {
                var addr = await _dbContext.UserAddresses.FirstOrDefaultAsync(a => a.Id == order.AddressId.Value && a.UserId == user.Id);
                if (addr != null)
                {
                    shippingAddress = new Address
                    {
                        ContactName = addr.FullName,
                        City = addr.City,
                        Country = "Turkey",
                        Description = $"{addr.District} {addr.Neighborhood} {addr.Details}",
                        ZipCode = addr.Id.ToString()
                    };
                    billingAddress = new Address
                    {
                        ContactName = addr.FullName,
                        City = addr.City,
                        Country = "Turkey",
                        Description = $"{addr.District} {addr.Neighborhood} {addr.Details}",
                        ZipCode = addr.Id.ToString()
                    };
                    var parts = (addr.FullName ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0) buyer.Name = parts.First();
                    if (parts.Length > 1) buyer.Surname = string.Join(" ", parts.Skip(1));
                    buyer.RegistrationAddress = shippingAddress.Description;
                    buyer.City = addr.City;
                }
            }

            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = order.OrderNumber,
                Price = (order.Total).ToString(System.Globalization.CultureInfo.InvariantCulture),
                PaidPrice = (order.Total).ToString(System.Globalization.CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                BasketId = order.OrderNumber,
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                Buyer = buyer,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                BasketItems = basketItems
            };

            request.PaymentCard = new PaymentCard
            {
                CardHolderName = cardHolderName,
                CardNumber = cardNumber?.Replace(" ", string.Empty),
                ExpireMonth = expireMonth,
                ExpireYear = expireYear,
                Cvc = cvc,
                RegisterCard = 0
            };

            var payment = await Payment.Create(request, options);
            if (payment.Status?.Equals("success", StringComparison.OrdinalIgnoreCase) == true)
            {
                var info = new PaymentInfo(
                    payment.PaymentId,
                    payment.ConversationId,
                    payment.Status,
                    payment.AuthCode,
                    payment.HostReference
                );
                return (true, null, info);
            }
            return (false, payment.ErrorMessage ?? payment.Status ?? "Ödeme başarısız oldu.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzipay payment exception for order {OrderNumber}", order.OrderNumber);
            return (false, "Ödeme sırasında bir hata oluştu.", null);
        }
    }
}


