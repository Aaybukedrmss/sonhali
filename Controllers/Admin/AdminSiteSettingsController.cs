using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers.Admin;

public class SiteSettingsVM
{
    public List<MenuItem> Navbar { get; set; } = new();
    public SocialLinks Footer { get; set; } = new();
}

public class MenuItem
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class SocialLinks
{
    public string Facebook { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string Youtube { get; set; } = string.Empty;
    public string Twitter { get; set; } = string.Empty;
}

[Authorize(Roles = "Admin")]
[Route("admin/settings")] 
public class AdminSiteSettingsController : Controller
{
    private readonly IWebHostEnvironment _env;
    private const string FileName = "site-settings.json";

    public AdminSiteSettingsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var path = Path.Combine(_env.ContentRootPath, FileName);
        SiteSettingsVM vm;
        if (System.IO.File.Exists(path))
        {
            var json = System.IO.File.ReadAllText(path);
            vm = JsonSerializer.Deserialize<SiteSettingsVM>(json) ?? new SiteSettingsVM();
        }
        else
        {
            vm = new SiteSettingsVM();
        }
        return View("~/Views/Admin/Settings/Index.cshtml", vm);
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public IActionResult Save(SiteSettingsVM model)
    {
        var path = Path.Combine(_env.ContentRootPath, FileName);
        var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(path, json);
        TempData["SuccessMessage"] = "Ayarlar kaydedildi";
        return RedirectToAction("Index");
    }
}


