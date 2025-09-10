using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly DataContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public OrdersController(DataContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var orders = await _dbContext.Orders
            .Where(o => o.UserId == user.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var orderIds = orders.Select(o => o.Id).ToList();
        var itemsLookup = await _dbContext.OrderItems
            .Include(oi => oi.Urun)
            .Where(oi => orderIds.Contains(oi.OrderId))
            .ToListAsync();

        // Adresler
        var addressIds = orders.Where(o => o.AddressId.HasValue).Select(o => o.AddressId!.Value).Distinct().ToList();
        var addresses = await _dbContext.UserAddresses
            .Where(a => addressIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a);

        var vm = orders.Select(o => new OrderListViewModel
        {
            Order = o,
            Items = itemsLookup.Where(i => i.OrderId == o.Id).ToList(),
            Address = o.AddressId.HasValue && addresses.ContainsKey(o.AddressId.Value) ? addresses[o.AddressId.Value] : null
        }).ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(string orderNumber)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == user.Id);
        if (order == null) return NotFound();

        if (order.Status == "Paid" || order.Status == "Pending")
        {
            order.Status = "Cancelled";
            await _dbContext.SaveChangesAsync();
            TempData["SuccessMessage"] = "Sipariş iptal edildi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Bu sipariş iptal edilemez.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Details(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber && o.UserId == user.Id);
        if (order == null) return NotFound();

        var items = await _dbContext.OrderItems
            .Include(i => i.Urun)
            .Where(i => i.OrderId == order.Id)
            .ToListAsync();

        UserAddress? address = null;
        if (order.AddressId.HasValue)
        {
            address = await _dbContext.UserAddresses.FirstOrDefaultAsync(a => a.Id == order.AddressId.Value);
        }

        var vm = new OrderDetailsViewModel
        {
            Order = order,
            Items = items,
            Address = address,
            TotalItems = items.Sum(i => i.Quantity),
            Shipments = 1
        };

        return View(vm);
    }
}

public class OrderListViewModel
{
    public Order Order { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public UserAddress? Address { get; set; }
}

public class OrderDetailsViewModel
{
    public Order Order { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public UserAddress? Address { get; set; }
    public int TotalItems { get; set; }
    public int Shipments { get; set; }
}


