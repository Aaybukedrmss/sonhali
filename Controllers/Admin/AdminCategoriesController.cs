using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/categories")] 
public class AdminCategoriesController : Controller
{
    private readonly DataContext _db;

    public AdminCategoriesController(DataContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Kategoriler.OrderBy(k => k.KategoriAdi).ToListAsync();
        return View("~/Views/Admin/Categories/Index.cshtml", list);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Categories/Create.cshtml", new KategoriCreateModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KategoriCreateModel model)
    {
        if (!ModelState.IsValid) return View("~/Views/Admin/Categories/Create.cshtml", model);
        var slug = string.IsNullOrWhiteSpace(model.Url) ? Slugify(model.KategoriAdi) : model.Url.Trim();
        var entity = new Kategori { KategoriAdi = model.KategoriAdi, Url = slug, Aktif = true };
        _db.Kategoriler.Add(entity);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var k = await _db.Kategoriler.FindAsync(id);
        if (k == null) return NotFound();
        var vm = new KategoriEditModel { Id = k.Id, KategoriAdi = k.KategoriAdi, Url = k.Url };
        return View("~/Views/Admin/Categories/Edit.cshtml", vm);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, KategoriEditModel model)
    {
        var k = await _db.Kategoriler.FindAsync(id);
        if (k == null) return NotFound();
        if (!ModelState.IsValid) return View("~/Views/Admin/Categories/Edit.cshtml", model);
        var slug = string.IsNullOrWhiteSpace(model.Url) ? Slugify(model.KategoriAdi) : model.Url.Trim();
        k.KategoriAdi = model.KategoriAdi; k.Url = slug;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "kategori";
        var normalized = text.Trim().ToLowerInvariant();
        normalized = normalized.Replace('ç', 'c').Replace('ğ', 'g').Replace('ı', 'i').Replace('ö', 'o').Replace('ş', 's').Replace('ü', 'u');
        var arr = normalized.Where(ch => char.IsLetterOrDigit(ch) || ch == ' ').ToArray();
        var cleaned = new string(arr);
        var slug = string.Join('-', cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? "kategori" : slug;
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var k = await _db.Kategoriler.FindAsync(id);
        if (k != null) { _db.Kategoriler.Remove(k); await _db.SaveChangesAsync(); }
        return RedirectToAction("Index");
    }
}


