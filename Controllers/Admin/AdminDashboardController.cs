using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_store.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminDashboardController : Controller
{
    [HttpGet("")]
    [HttpGet("dashboard")]
    public IActionResult Index()
    {
        return View("~/Views/Admin/Dashboard/Index.cshtml");
    }
}


