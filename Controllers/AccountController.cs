using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using dotnet_store.Models;

namespace dotnet_store.Controllers;

public class AccountController : Controller
{
    private UserManager<AppUser> _userManager;
    private SignInManager<AppUser> _signInManager;
    private readonly DataContext _dbContext;
    private readonly ILogger<AccountController> _logger;
    
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, DataContext dbContext, ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public ActionResult Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<ActionResult> Create(AccountCreateModel model)
    {
        try
        {
            _logger.LogInformation("Kayıt işlemi başladı: {Username}, {Email}", model?.Username, model?.Email);
            
            if(ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    AdSoyad = model.AdSoyad
                };
                
                _logger.LogInformation("AppUser oluşturuldu, UserManager.CreateAsync çağrılıyor...");
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if(result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı başarıyla oluşturuldu: {Username}", model.Username);
                    TempData["SuccessMessage"] = "Hesabınız başarıyla oluşturuldu!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Kullanıcı oluşturma başarısız: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    foreach(var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            else
            {
                _logger.LogWarning("ModelState geçersiz: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt işlemi sırasında hata oluştu");
            ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
        }
        
        return View(model);
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View(new AccountLoginModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Login(AccountLoginModel model)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        try
        {
            AppUser? user = await _userManager.FindByNameAsync(model.UsernameOrEmail);
            if(user == null && model.UsernameOrEmail.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(model.UsernameOrEmail);
            }

            if(user == null)
            {
                _dbContext.LoginLogs.Add(new LoginLog
                {
                    UserId = null,
                    UsernameOrEmail = model.UsernameOrEmail,
                    IsSuccess = false,
                    AttemptedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ErrorMessage = "Kullanıcı bulunamadı"
                });
                await _dbContext.SaveChangesAsync();

                ModelState.AddModelError("", "Kullanıcı adı/e-posta veya şifre hatalı.");
                return View(model);
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);

            _dbContext.LoginLogs.Add(new LoginLog
            {
                UserId = user.Id,
                UsernameOrEmail = model.UsernameOrEmail,
                IsSuccess = signInResult.Succeeded,
                AttemptedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ErrorMessage = signInResult.Succeeded ? null : "Hatalı giriş"
            });
            await _dbContext.SaveChangesAsync();

            if(signInResult.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Kullanıcı adı/e-posta veya şifre hatalı.");
            return View(model);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Giriş sırasında hata oluştu");

            _dbContext.LoginLogs.Add(new LoginLog
            {
                UserId = null,
                UsernameOrEmail = model.UsernameOrEmail,
                IsSuccess = false,
                AttemptedAt = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ErrorMessage = ex.Message
            });
            await _dbContext.SaveChangesAsync();

            ModelState.AddModelError("", "Giriş işlemi sırasında bir hata oluştu.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if(user == null)
        {
            // Güvenlik için aynı cevap
            ViewBag.ResetLink = null;
            return View("ForgotPasswordSent");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action(
            action: "ResetPassword",
            controller: "Account",
            values: new { email = model.Email, token = token },
            protocol: Request.Scheme
        );

        ViewBag.ResetLink = resetLink;
        return View("ForgotPasswordSent");
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return BadRequest();
        }

        var vm = new ResetPasswordModel
        {
            Email = email,
            Token = token
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if(user == null)
        {
            TempData["SuccessMessage"] = "Şifreniz güncellendi.";
            return RedirectToAction("Login");
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if(result.Succeeded)
        {
            TempData["SuccessMessage"] = "Şifre başarıyla güncellendi.";
            return RedirectToAction("Login");
        }

        foreach(var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return View(model);
    }
}