using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/roles")] 
public class AdminRolesController : Controller
{
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public AdminRolesController(RoleManager<IdentityRole<int>> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var roles = _roleManager.Roles.ToList();
        return View("~/Views/Admin/Roles/Index.cshtml", roles);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["ErrorMessage"] = "Rol adı zorunludur";
            return RedirectToAction("Index");
        }
        if (!await _roleManager.RoleExistsAsync(name))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole<int>(name));
            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Rol oluşturuldu" : string.Join(", ", result.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["ErrorMessage"] = "Rol zaten mevcut";
        }
        return RedirectToAction("Index");
    }

    [HttpPost("assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(string usernameOrEmail, string role)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(role))
        {
            TempData["ErrorMessage"] = "Kullanıcı ve rol zorunludur";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByNameAsync(usernameOrEmail);
        user ??= await _userManager.FindByEmailAsync(usernameOrEmail);
        if (user == null)
        {
            TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
            return RedirectToAction("Index");
        }
        if (!await _roleManager.RoleExistsAsync(role))
        {
            TempData["ErrorMessage"] = "Rol bulunamadı";
            return RedirectToAction("Index");
        }
        var result = await _userManager.AddToRoleAsync(user, role);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Succeeded ? "Rol atandı" : string.Join(", ", result.Errors.Select(e => e.Description));
        return RedirectToAction("Index");
    }
}


