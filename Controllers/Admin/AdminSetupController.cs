using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;

namespace dotnet_store.Controllers.Admin;

[Authorize]
[Route("admin/setup")]
public class AdminSetupController : Controller
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public AdminSetupController(RoleManager<IdentityRole<int>> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Views/Admin/Setup/Index.cshtml");
    }

    [HttpPost("make-me-admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeMeAdmin()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }
        var res = await _userManager.AddToRoleAsync(user, "Admin");
        TempData[res.Succeeded ? "SuccessMessage" : "ErrorMessage"] = res.Succeeded ? "Artık yöneticisiniz" : string.Join(", ", res.Errors.Select(e => e.Description));
        return RedirectToAction("Index", "AdminDashboard");
    }
}


