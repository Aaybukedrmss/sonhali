using System.Text.RegularExpressions;
using dotnet_store.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/sliders")] 
public class AdminSlidersController : Controller
{
    private readonly DataContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminSlidersController(DataContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var sliders = await _db.Sliders.OrderBy(s => s.Index).ToListAsync();
        return View("~/Views/Admin/Sliders/Index.cshtml", sliders);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View("~/Views/Admin/Sliders/Create.cshtml", new SliderCreateModel { Aktif = true });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SliderCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Sliders/Create.cshtml", model);
        }

        string? fileName = await SaveImageAsync(model.Resim, subFolder: "sliders");
        if (fileName == null)
        {
            ModelState.AddModelError("Resim", "Resim yüklenemedi");
            return View("~/Views/Admin/Sliders/Create.cshtml", model);
        }

        var entity = new Slider
        {
            Baslik = model.Baslik,
            Aciklama = model.Aciklama,
            Resim = $"img/sliders/{fileName}",
            Index = model.Index,
            Aktif = model.Aktif
        };
        _db.Sliders.Add(entity);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var s = await _db.Sliders.FindAsync(id);
        if (s == null) return NotFound();
        var vm = new SliderEditModel
        {
            Id = s.Id,
            Baslik = s.Baslik,
            Aciklama = s.Aciklama,
            ResimAdi = s.Resim,
            Index = s.Index,
            Aktif = s.Aktif
        };
        return View("~/Views/Admin/Sliders/Edit.cshtml", vm);
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SliderEditModel model)
    {
        var s = await _db.Sliders.FindAsync(id);
        if (s == null) return NotFound();
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Sliders/Edit.cshtml", model);
        }

        s.Baslik = model.Baslik;
        s.Aciklama = model.Aciklama;
        s.Index = model.Index;
        s.Aktif = model.Aktif;

        if (model.Resim != null)
        {
            string? fileName = await SaveImageAsync(model.Resim, subFolder: "sliders");
            if (fileName != null)
            {
                s.Resim = $"img/sliders/{fileName}";
            }
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Sliders.FindAsync(id);
        if (s != null)
        {
            _db.Sliders.Remove(s);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    private async Task<string?> SaveImageAsync(IFormFile? file, string subFolder)
    {
        if (file == null || file.Length == 0) return null;
        var uploadsDir = Path.Combine(_env.WebRootPath, "img", subFolder);
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        var safeName = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName), "[^a-zA-Z0-9-_]", "-").ToLowerInvariant();
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{safeName}-{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(uploadsDir, fileName);
        using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }
        return fileName;
    }
}


