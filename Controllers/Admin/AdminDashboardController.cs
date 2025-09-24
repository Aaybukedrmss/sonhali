using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using dotnet_store.Models;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin,Support,Manager")] // Admin, Support ve Manager rolleri erişebilir
[Route("admin")]
public class AdminDashboardController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public AdminDashboardController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isSupport = await _userManager.IsInRoleAsync(user, "Support");
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

        // Destek rolü için özel dashboard (Admin değilse)
        if (isSupport && !isAdmin)
        {
            return View("~/Views/Admin/Dashboard/Support.cshtml");
        }
        
        // Admin ve Manager için normal dashboard
        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}


