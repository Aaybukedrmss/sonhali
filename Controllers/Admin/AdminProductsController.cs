using System.Text.RegularExpressions;
using dotnet_store.Models;
using dotnet_store.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/products")] 
public class AdminProductsController : Controller
{
    private readonly DataContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminProductsController(DataContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Urunler.OrderByDescending(p => p.Id).Take(500).ToListAsync();
        return View("~/Views/Admin/Products/Index.cshtml", list);
    }

    [HttpGet("create")]
    [Authorize(Roles = "Admin,Editor")] // Admin ve Editor rolleri ürün oluşturabilir
    public IActionResult Create()
    {
        return View("~/Views/Admin/Products/Create.cshtml", new AdminProductFormModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")] // Admin ve Editor rolleri ürün oluşturabilir
    public async Task<IActionResult> Create(AdminProductFormModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Form hataları var. Lütfen kontrol edin.";
            return View("~/Views/Admin/Products/Create.cshtml", model);
        }

        var entity = new Urun
        {
            ProductName = model.ProductName,
            Sku = model.Sku,
            Isbn = model.Isbn,
            LeadTimeHour = model.LeadTimeHour,
            Fiyat = model.Fiyat,
            Aktif = model.Aktif ? (byte)1 : (byte)0,
            Degerlendirmelerim = model.Degerlendirmelerim,
            StockQuantity = model.StockQuantity,
            FullDescription = model.FullDescription,
            Yayinevi = model.Yayinevi,
            Marka = model.Marka,
            MedyaTipi = model.MedyaTipi,
            TopMostCategoryId = model.TopMostCategoryId,
            TopMostCategoryNames = model.TopMostCategoryNames,
            CategoryId = model.CategoryId,
            CategorySlug = model.CategorySlug,
            TotalRating = model.TotalRating
        };
        _db.Urunler.Add(entity);
        await _db.SaveChangesAsync();

        if (model.Resim != null)
        {
            var baseName = entity.Isbn != 0 ? entity.Isbn.ToString() : entity.Id.ToString();
            await SaveProductImageAsync(model.Resim, baseName);
        }

        TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
        return Redirect("/admin/products");
    }

    [HttpGet("edit/{id:int}")]
    [Authorize(Roles = "Admin,Editor")] // Admin ve Editor rolleri ürün düzenleyebilir
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Urunler.FindAsync(id);
        if (p == null) return NotFound();
        var vm = new AdminProductFormModel
        {
            ProductName = p.ProductName,
            Sku = p.Sku,
            Isbn = p.Isbn,
            LeadTimeHour = p.LeadTimeHour,
            Fiyat = p.Fiyat,
            Aktif = p.Aktif == 1,
            Degerlendirmelerim = p.Degerlendirmelerim,
            StockQuantity = p.StockQuantity,
            FullDescription = p.FullDescription,
            Yayinevi = p.Yayinevi,
            Marka = p.Marka,
            MedyaTipi = p.MedyaTipi,
            TopMostCategoryId = p.TopMostCategoryId,
            TopMostCategoryNames = p.TopMostCategoryNames,
            CategoryId = p.CategoryId,
            CategorySlug = p.CategorySlug,
            TotalRating = p.TotalRating
        };
        ViewData["ProductId"] = id;
        return View("~/Views/Admin/Products/Edit.cshtml", vm);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Editor")] // Admin ve Editor rolleri ürün düzenleyebilir
    public async Task<IActionResult> Edit(int id, AdminProductFormModel model)
    {
        var p = await _db.Urunler.FindAsync(id);
        if (p == null) return NotFound();
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Form hataları var. Lütfen kontrol edin.";
            ViewData["ProductId"] = id;
            return View("~/Views/Admin/Products/Edit.cshtml", model);
        }

        p.ProductName = model.ProductName;
        p.Sku = model.Sku;
        p.Isbn = model.Isbn;
        p.LeadTimeHour = model.LeadTimeHour;
        p.Fiyat = model.Fiyat;
        p.Aktif = model.Aktif ? (byte)1 : (byte)0;
        p.Degerlendirmelerim = model.Degerlendirmelerim;
        p.StockQuantity = model.StockQuantity;
        p.FullDescription = model.FullDescription;
        p.Yayinevi = model.Yayinevi;
        p.Marka = model.Marka;
        p.MedyaTipi = model.MedyaTipi;
        p.TopMostCategoryId = model.TopMostCategoryId;
        p.TopMostCategoryNames = model.TopMostCategoryNames;
        p.CategoryId = model.CategoryId;
        p.CategorySlug = model.CategorySlug;
        p.TotalRating = model.TotalRating;
        await _db.SaveChangesAsync();

        if (model.Resim != null)
        {
            var baseName = p.Isbn != 0 ? p.Isbn.ToString() : p.Id.ToString();
            await SaveProductImageAsync(model.Resim, baseName);
        }

        TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
        return Redirect("/admin/products");
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")] // Sadece Admin rolü ürün silebilir
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Urunler.FindAsync(id);
        if (p != null)
        {
            _db.Urunler.Remove(p);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Ürün silindi.";
        }
        else
        {
            TempData["ErrorMessage"] = "Ürün bulunamadı.";
        }
        return Redirect("/admin/products");
    }

    private async Task<string?> SaveProductImageAsync(IFormFile file, string baseName)
    {
        if (file == null || file.Length == 0) return null;
        var uploadsDir = Path.Combine(_env.WebRootPath, "img", "products");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeBase = Regex.Replace(baseName, "[^a-zA-Z0-9-_]", "-");
        var fileName = $"{safeBase}{ext}";
        var path = Path.Combine(uploadsDir, fileName);
        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        return fileName;
    }
}


