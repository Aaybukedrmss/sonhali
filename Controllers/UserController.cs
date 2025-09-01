using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using dotnet_store.Models;

namespace dotnet_store.Controllers;

public class UserController : Controller
{
    private UserManager<IdentityUser> _userManager;
    
    public UserController(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<ActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }
    
    public async Task<ActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }
    
    [HttpPost]
    public async Task<ActionResult> Edit(string id, IdentityUser model)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        
        user.UserName = model.UserName;
        user.Email = model.Email;
        
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Index");
        }
        
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        
        return View(user);
    }
}
