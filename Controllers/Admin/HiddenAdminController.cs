using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using dotnet_store.Models;

namespace dotnet_store.Controllers.Admin;

[Route("__admin")] // gizli yol
public class HiddenAdminController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _config;

    public HiddenAdminController(UserManager<AppUser> userManager,
                                 RoleManager<IdentityRole<int>> roleManager,
                                 SignInManager<AppUser> signInManager,
                                 IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _config = config;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Views/Admin/HiddenAdmin/Index.cshtml");
    }

    [HttpGet("generate")]
    public IActionResult Generate()
    {
        // GET: /__admin/generate -> formu da gösterebilsin
        return View("~/Views/Admin/HiddenAdmin/Index.cshtml");
    }

    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ErrorMessage"] = "Email zorunlu";
            return RedirectToAction("Index");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser { UserName = email.Split('@')[0], Email = email, AdSoyad = email };
            var createRes = await _userManager.CreateAsync(user, GenerateStrongPassword());
            if (!createRes.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join(", ", createRes.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }
        }

        var password = GenerateStrongPassword();
        // Token providers yoksa doğrudan şifre hash'ini güncelle
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);
        var resetRes = await _userManager.UpdateAsync(user);
        if (!resetRes.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(", ", resetRes.Errors.Select(e => e.Description));
            return RedirectToAction("Index");
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }
        await _userManager.AddToRoleAsync(user, "Admin");

        var sent = await TrySendEmailAsync(email, "Admin Giriş Bilgileri",
            $"Merhaba,\n\nGeçici admin şifreniz: {password}\nGiriş: /Account/Login\nAdmin Paneli: /admin\n\nLütfen giriş yaptıktan sonra şifrenizi değiştirin.");

        ViewBag.EmailSent = sent;
        ViewBag.GeneratedPassword = password;
        ViewBag.Email = email;
        return View("~/Views/Admin/HiddenAdmin/Done.cshtml");
    }

    private static string GenerateStrongPassword()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "A").Replace("/", "Z").Substring(0, 12) + "!9a";
    }

    private async Task<bool> TrySendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Pass"];
            var from = _config["Smtp:From"] ?? user;
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                return false;
            }
            int port = 587;
            int.TryParse(portStr, out port);
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };
            var mail = new MailMessage(from!, to, subject, body);
            await client.SendMailAsync(mail);
            return true;
        }
        catch
        {
            return false;
        }
    }
}


