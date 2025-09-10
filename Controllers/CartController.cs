using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly DataContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<CartController> _logger;

    public CartController(DataContext dbContext, UserManager<AppUser> userManager, ILogger<CartController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var items = await _dbContext.CartItems
            .Include(ci => ci.Urun)
            .Where(ci => ci.UserId == user.Id)
            .OrderBy(ci => ci.CreatedAt)
            .ToListAsync();

        var vm = new CartIndexViewModel
        {
            Items = items,
            Subtotal = items.Sum(i => i.UnitPrice * i.Quantity)
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var items = await _dbContext.CartItems
            .Include(ci => ci.Urun)
            .Where(ci => ci.UserId == user.Id)
            .ToListAsync();

        var addresses = await _dbContext.UserAddresses
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Id)
            .ToListAsync();

        var vm = new CheckoutViewModel
        {
            Items = items,
            Subtotal = items.Sum(i => i.UnitPrice * i.Quantity),
            Addresses = addresses,
            SelectedAddressId = addresses.FirstOrDefault(a => a.IsDefault)?.Id ?? addresses.FirstOrDefault()?.Id ?? 0,
            ShippingProvider = "Yurtici",
            ShippingCost = CalculateShippingCost("Yurtici", items),
            ShippingOptions = GetShippingOptions(items)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var items = await _dbContext.CartItems
            .Include(ci => ci.Urun)
            .Where(ci => ci.UserId == user.Id)
            .ToListAsync();

        if (!items.Any())
        {
            TempData["ErrorMessage"] = "Sepetiniz boş.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
        {
            var addresses = await _dbContext.UserAddresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .ToListAsync();

            model.Items = items;
            model.Subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
            model.ShippingCost = CalculateShippingCost(model.ShippingProvider, items);
            model.ShippingOptions = GetShippingOptions(items);
            model.Addresses = addresses;
            return View(model);
        }

        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var shipping = CalculateShippingCost(model.ShippingProvider, items);
        var total = subtotal + shipping;

        var order = new Order
        {
            UserId = user.Id,
            OrderNumber = GenerateOrderNumber(),
            ShippingProvider = model.ShippingProvider,
            AddressId = model.SelectedAddressId == 0 ? null : model.SelectedAddressId,
            Subtotal = subtotal,
            ShippingCost = shipping,
            Total = total,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Sipariş kalemleri
        foreach (var ci in items)
        {
            _dbContext.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                UrunId = ci.UrunId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice
            });
        }
        await _dbContext.SaveChangesAsync();

        // Ödemeye yönlendir
        return RedirectToAction("Pay", "Payment", new { orderNumber = order.OrderNumber });
    }

    [HttpGet]
    public IActionResult Confirmation(string provider, string? orderNumber, decimal? total)
    {
        ViewBag.Provider = provider;
        ViewBag.OrderNumber = orderNumber;
        ViewBag.Total = total;
        return View();
    }

    private static decimal CalculateShippingCost(string provider, List<CartItem> items)
    {
        // Basit örnek fiyatlandırma
        var weightFactor = Math.Max(1, items.Sum(i => i.Quantity));
        return provider?.ToLowerInvariant() switch
        {
            "yurtici" => 29.90m + 2m * weightFactor,
            "aras" => 24.90m + 2.5m * weightFactor,
            "mng" => 19.90m + 3m * weightFactor,
            _ => 29.90m
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"SC{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100,999)}";
    }

    private static List<ShippingOption> GetShippingOptions(List<CartItem> items)
    {
        return new List<ShippingOption>
        {
            new ShippingOption { Provider = "Yurtici", DisplayName = "Yurtiçi Kargo", Cost = CalculateShippingCost("Yurtici", items) },
            new ShippingOption { Provider = "Aras", DisplayName = "Aras Kargo", Cost = CalculateShippingCost("Aras", items) },
            new ShippingOption { Provider = "Mng", DisplayName = "MNG Kargo", Cost = CalculateShippingCost("Mng", items) },
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int urunId, int quantity = 1)
    {
        if (quantity < 1)
        {
            quantity = 1;
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var urun = await _dbContext.Urunler.FirstOrDefaultAsync(u => u.Id == urunId);
        if (urun == null)
        {
            return NotFound();
        }

        var existing = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.UserId == user.Id && ci.UrunId == urunId);
        if (existing == null)
        {
            var item = new CartItem
            {
                UserId = user.Id,
                UrunId = urun.Id,
                Quantity = quantity,
                UnitPrice = (decimal)urun.Fiyat,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.CartItems.Add(item);
        }
        else
        {
            existing.Quantity += quantity;
            existing.UnitPrice = (decimal)urun.Fiyat;
            _dbContext.CartItems.Update(existing);
        }

        await _dbContext.SaveChangesAsync();
        TempData["SuccessMessage"] = "Ürün sepete eklendi.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int itemId, int quantity)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == user.Id);
        if (item == null)
        {
            return NotFound();
        }

        if (quantity < 1)
        {
            _dbContext.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            _dbContext.CartItems.Update(item);
        }

        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var item = await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == user.Id);
        if (item == null)
        {
            return NotFound();
        }

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}


