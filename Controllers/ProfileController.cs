using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_store.Models;
using dotnet_store.Models.Account;

namespace dotnet_store.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly DataContext _db;

    public ProfileController(UserManager<AppUser> userManager, DataContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        ViewBag.FullName = user?.AdSoyad ?? user?.UserName;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileUpdateModel model)
    {
        if(!ModelState.IsValid)
        {
            TempData["ProfileError"] = "Lütfen gerekli alanları doğru doldurun.";
            return RedirectToAction("Index");
        }

        var user = await _userManager.GetUserAsync(User);
        if(user == null)
        {
            return Challenge();
        }

        // E-posta ve kullanıcı adı eşsizliği kontrolü
        var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
        if(existingByEmail != null && existingByEmail.Id != user.Id)
        {
            TempData["ProfileError"] = "Bu e-posta başka bir kullanıcı tarafından kullanılıyor.";
            return RedirectToAction("Index");
        }

        var existingByUsername = await _userManager.FindByNameAsync(model.Username);
        if(existingByUsername != null && existingByUsername.Id != user.Id)
        {
            TempData["ProfileError"] = "Bu kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.";
            return RedirectToAction("Index");
        }

        user.UserName = model.Username;
        user.Email = model.Email;
        user.AdSoyad = string.Join(' ', new[]{model.FirstName?.Trim(), model.LastName?.Trim()}.Where(s=>!string.IsNullOrWhiteSpace(s)));
        user.PhoneNumber = model.Phone;

        var result = await _userManager.UpdateAsync(user);
        if(!result.Succeeded)
        {
            TempData["ProfileError"] = string.Join(" ", result.Errors.Select(e=>e.Description));
            return RedirectToAction("Index");
        }

        TempData["ProfileSuccess"] = "Bilgileriniz güncellendi.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Addresses()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();

        var list = await _db.UserAddresses
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new {
                a.Id,
                a.Title,
                a.FullName,
                a.Phone,
                a.City,
                a.District,
                a.Neighborhood,
                a.Details,
                a.IsDefault
            }).ToListAsync();
        return Json(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Address([FromBody] UserAddressSaveModel model)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();

        if(model.IsDefault)
        {
            var existingDefaults = await _db.UserAddresses.Where(a=>a.UserId==user.Id && a.IsDefault).ToListAsync();
            foreach(var a in existingDefaults){ a.IsDefault = false; }
        }

        var entity = new UserAddress
        {
            UserId = user.Id,
            Title = model.Title,
            FullName = model.FullName,
            Phone = model.Phone,
            City = model.City,
            District = model.District,
            Neighborhood = model.Neighborhood,
            Details = model.Details,
            IsDefault = model.IsDefault,
            CreatedAt = DateTime.UtcNow
        };
        _db.UserAddresses.Add(entity);
        await _db.SaveChangesAsync();

        return Json(new { success = true, id = entity.Id });
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Address(int id, [FromBody] UserAddressSaveModel model)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();

        var entity = await _db.UserAddresses.FirstOrDefaultAsync(a=>a.Id==id && a.UserId==user.Id);
        if(entity == null) return NotFound();

        if(model.IsDefault)
        {
            var existingDefaults = await _db.UserAddresses.Where(a=>a.UserId==user.Id && a.IsDefault && a.Id!=id).ToListAsync();
            foreach(var a in existingDefaults){ a.IsDefault = false; }
        }

        entity.Title = model.Title;
        entity.FullName = model.FullName;
        entity.Phone = model.Phone;
        entity.City = model.City;
        entity.District = model.District;
        entity.Neighborhood = model.Neighborhood;
        entity.Details = model.Details;
        entity.IsDefault = model.IsDefault;
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Address(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();
        var entity = await _db.UserAddresses.FirstOrDefaultAsync(a=>a.Id==id && a.UserId==user.Id);
        if(entity == null) return NotFound();
        _db.UserAddresses.Remove(entity);
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();
        var entity = await _db.UserAddresses.FirstOrDefaultAsync(a=>a.Id==id && a.UserId==user.Id);
        if(entity == null) return NotFound();
        var list = await _db.UserAddresses.Where(a=>a.UserId==user.Id).ToListAsync();
        foreach(var a in list){ a.IsDefault = a.Id == id; }
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }
}


