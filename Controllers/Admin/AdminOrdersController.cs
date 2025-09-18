using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/orders")] 
public class AdminOrdersController : Controller
{
    private readonly DataContext _db;

    public AdminOrdersController(DataContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? status)
    {
        var query = _db.Orders.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(o => o.OrderNumber.Contains(q));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }
        var orders = await query.OrderByDescending(o => o.CreatedAt).Take(500).ToListAsync();
        return View("~/Views/Admin/Orders/Index.cshtml", orders);
    }

    [HttpGet("details/{orderNumber}")]
    public async Task<IActionResult> Details(string orderNumber)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        if (order == null) return NotFound();
        var items = await _db.OrderItems.Include(i => i.Urun).Where(i => i.OrderId == order.Id).ToListAsync();
        var vm = new AdminOrderDetailsVM { Order = order, Items = items };
        return View("~/Views/Admin/Orders/Details.cshtml", vm);
    }

    [HttpPost("update-status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(string orderNumber, string status)
    {
        var allowed = new[] { "Yeni", "Onay", "Kargoya Verildi", "Iptal", "Iade" };
        if (!allowed.Contains(status))
        {
            TempData["ErrorMessage"] = "Geçersiz durum";
            return RedirectToAction("Index");
        }
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Sipariş bulunamadı";
            return RedirectToAction("Index");
        }
        order.Status = status;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Durum güncellendi";
        return RedirectToAction("Details", new { orderNumber });
    }
}

public class AdminOrderDetailsVM
{
    public Order Order { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
}


