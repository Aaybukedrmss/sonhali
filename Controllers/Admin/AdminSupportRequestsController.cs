using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin,Support")] // Admin ve Support rolleri destek panelini görebilir
[Route("admin/support")] 
public class AdminSupportRequestsController : Controller
{
    private readonly DataContext _db;

    public AdminSupportRequestsController(DataContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? type)
    {
        var query = _db.SupportRequests
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(s => s.Type == type);
            ViewData["FilterType"] = type;
        }

        var list = await query.Take(500).ToListAsync();
        return View("~/Views/Admin/SupportRequests/Index.cshtml", list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.SupportRequests.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (item == null) return NotFound();
        return View("~/Views/Admin/SupportRequests/Details.cshtml", item);
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.SupportRequests.FindAsync(id);
        if (item != null)
        {
            _db.SupportRequests.Remove(item);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kayıt silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Kayıt bulunamadı.";
        }
        return Redirect("/admin/support");
    }
}


