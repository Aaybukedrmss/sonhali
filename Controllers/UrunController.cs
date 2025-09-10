using dotnet_store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

public class UrunController : Controller
{
    private readonly DataContext _context;
    private readonly ILogger<UrunController> _logger;

    public UrunController(DataContext context, ILogger<UrunController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ActionResult> Index()
    {
        try
        {
            var urunler = await _context.Urunler.ToListAsync();
            return View(urunler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün listesi alınırken hata oluştu");
            TempData["ErrorMessage"] = "Ürünler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            return View(new List<Urun>());
        }
    }


    public async Task<ActionResult> Details(int id)
    {
        try
        {
            var urun = await _context.Urunler.FirstOrDefaultAsync(u => u.Id == id);

            if (urun == null)
            {
                return NotFound();
            }

            // Benzer ürünleri getir (mevcut ürün hariç, rastgele 4 tane)
            var benzerUrunler = await _context.Urunler
                .Where(u => u.Id != id) // Mevcut ürünü hariç tut
                .OrderBy(r => Guid.NewGuid()) // Rastgele sıralama
                .Take(4) // İlk 4 ürünü al
                .ToListAsync();

            ViewBag.BenzerUrunler = benzerUrunler;

            return View(urun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün detayları alınırken hata oluştu. ID: {Id}", id);
            TempData["ErrorMessage"] = "Ürün detayları yüklenirken bir hata oluştu.";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<ActionResult> List(string? kategori, string? yayinevi, string? yazar, string? marka, 
        double? minFiyat, double? maxFiyat, double? minRating)
    {
        try
        {
            var query = _context.Urunler.AsQueryable();

            // Filtreleri uygula
            if (!string.IsNullOrEmpty(kategori))
            {
                query = query.Where(u => u.TopMostCategoryNames.Contains(kategori));
            }

            if (!string.IsNullOrEmpty(yayinevi))
            {
                query = query.Where(u => u.Yayinevi.Contains(yayinevi));
            }

            if (!string.IsNullOrEmpty(yazar))
            {
                query = query.Where(u => u.Yazar.Contains(yazar));
            }

            if (!string.IsNullOrEmpty(marka))
            {
                query = query.Where(u => u.Marka.Contains(marka));
            }

            if (minFiyat.HasValue)
            {
                query = query.Where(u => u.Fiyat >= minFiyat.Value);
            }

            if (maxFiyat.HasValue)
            {
                query = query.Where(u => u.Fiyat <= maxFiyat.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(u => u.TotalRating <= minRating.Value);
            }

            var urunler = await query.ToListAsync();

            // Filtre seçeneklerini view'a gönder
            ViewBag.Kategoriler = await _context.Urunler.Select(u => u.TopMostCategoryNames).Distinct().ToListAsync();
            ViewBag.Yayinevleri = await _context.Urunler.Select(u => u.Yayinevi).Distinct().ToListAsync();
            ViewBag.Yazarlar = await _context.Urunler.Select(u => u.Yazar).Distinct().ToListAsync();
            ViewBag.Markalar = await _context.Urunler.Select(u => u.Marka).Distinct().ToListAsync();

            return View(urunler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ürün listesi alınırken hata oluştu");
            TempData["ErrorMessage"] = "Ürünler yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            return View(new List<Urun>());
        }
    }
}
