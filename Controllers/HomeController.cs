using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using dotnet_store.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_store.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var urun = await _context.Urunler.Take(8).ToListAsync();
            return View(urun);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabanı bağlantı hatası");
            ViewBag.Error = "Veritabanı bağlantısında hata oluştu: " + ex.Message;
            return View(new List<Urun>());
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
