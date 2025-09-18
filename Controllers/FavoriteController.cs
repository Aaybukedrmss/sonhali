using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

[Authorize]
public class FavoriteController : Controller
{
    private readonly DataContext _dbContext;
    private readonly UserManager<AppUser> _userManager;

    public FavoriteController(DataContext dbContext, UserManager<AppUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var favorites = await _dbContext.Favorites
            .Where(f => f.UserId == user.Id)
            .Include(f => f.Urun)
            .ToListAsync();

        var urunler = favorites.Select(f => f.Urun).ToList();
        return View(urunler);
    }

    // Toggle action kaldırıldı - artık sadece ToggleAjax kullanılıyor

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAjax(int urunId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var existing = await _dbContext.Favorites
            .FirstOrDefaultAsync(f => f.UserId == user.Id && f.UrunId == urunId);

        bool isFavorite;
        if (existing != null)
        {
            _dbContext.Favorites.Remove(existing);
            isFavorite = false;
        }
        else
        {
            _dbContext.Favorites.Add(new Favorite
            {
                UserId = user.Id,
                UrunId = urunId
            });
            isFavorite = true;
        }

        await _dbContext.SaveChangesAsync();
        return Json(new { success = true, isFavorite });
    }
}


