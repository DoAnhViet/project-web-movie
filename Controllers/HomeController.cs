using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MovieApiService _movieApiService;

    public HomeController(ILogger<HomeController> logger, MovieApiService movieApiService)
    {
        _logger = logger;
        _movieApiService = movieApiService;
    }

    public IActionResult Index()
    {
        // Trang chủ đơn giản - frontend team sẽ làm UI
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
