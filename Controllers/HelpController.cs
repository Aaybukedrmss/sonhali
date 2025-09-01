using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using dotnet_store.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace dotnet_store.Controllers;

public class HelpController : Controller
{
    private readonly DataContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    [ActivatorUtilitiesConstructor]
    public HelpController(DataContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    public class HelpSubmitModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string Type { get; set; } = null!; // Yardim, Sikayet, Oneri

        [Required]
        public string Message { get; set; } = null!;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(HelpSubmitModel model)
    {
        if(!ModelState.IsValid)
        {
            TempData["HelpError"] = "Lütfen zorunlu alanları doldurun.";
            return RedirectToAction("Index");
        }

        var support = new SupportRequest
        {
            Email = model.Email,
            Type = model.Type,
            Message = model.Message
        };

        var user = await _userManager.GetUserAsync(User);
        if(user != null)
        {
            support.UserId = user.Id;
        }

        _dbContext.SupportRequests.Add(support);
        await _dbContext.SaveChangesAsync();

        TempData["HelpSuccess"] = "Talebiniz kaydedildi. En kısa sürede dönüş yapacağız.";
        return RedirectToAction("Index");
    }
}


