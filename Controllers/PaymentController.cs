using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using dotnet_store.Services;

namespace dotnet_store.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly DataContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<PaymentController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentController(DataContext dbContext, UserManager<AppUser> userManager, ILogger<PaymentController> logger, IPaymentService paymentService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Pay(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return RedirectToAction("Index", "Cart");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == user.Id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Ödeme bulunamadı.";
            return RedirectToAction("Index", "Cart");
        }

        ViewBag.OrderNumber = order.OrderNumber;
        ViewBag.Total = order.Total;
        ViewBag.Provider = order.ShippingProvider;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(string orderNumber, string cardNumber, string cardName, string expiry, string cvc)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == user.Id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Ödeme bulunamadı.";
            return RedirectToAction("Index", "Cart");
        }

        try
        {
            // Kart son kullanma
            string expMonth = "";
            string expYear = "";
            if (!string.IsNullOrWhiteSpace(expiry))
            {
                var parts = expiry.Replace(" ", string.Empty).Split('/', '-');
                if (parts.Length == 2)
                {
                    expMonth = parts[0];
                    expYear = parts[1].Length == 2 ? $"20{parts[1]}" : parts[1];
                }
            }
            var (success, error, info) = await _paymentService.ChargeOrderAsync(
                order,
                user,
                cardName,
                cardNumber,
                expMonth,
                expYear,
                cvc,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            if (success)
            {
                // Ödeme başarılıysa sepeti temizle
                var cartItems = await _dbContext.CartItems.Where(ci => ci.UserId == user.Id).ToListAsync();
                _dbContext.CartItems.RemoveRange(cartItems);
                await _dbContext.SaveChangesAsync();

                order.Status = "Paid";
                order.PaymentStatus = "success";
                if (info != null)
                {
                    order.PaymentId = info.PaymentId;
                    order.ConversationId = info.ConversationId;
                }
                await _dbContext.SaveChangesAsync();

                return RedirectToAction("Confirmation", "Cart", new { provider = order.ShippingProvider, orderNumber = order.OrderNumber, total = order.Total, paymentId = info?.PaymentId, conv = info?.ConversationId, auth = info?.AuthCode });
            }
            else
            {
                _logger.LogWarning("Iyzipay payment failed. Order {OrderNumber}. Error: {ErrorMessage}", order.OrderNumber, error);
                TempData["ErrorMessage"] = error ?? "Ödeme başarısız oldu.";
                // failure durumunu da siparişe yazalım
                order.PaymentStatus = "failure";
                order.PaymentLastError = error;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction("Pay", new { orderNumber });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Iyzipay payment exception for order {OrderNumber}", order.OrderNumber);
            TempData["ErrorMessage"] = "Ödeme sırasında bir hata oluştu.";
            return RedirectToAction("Pay", new { orderNumber });
        }
    }
}


