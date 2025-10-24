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

    public async Task<IActionResult> Index(int page = 1)
    {
        var moviesResponse = await _movieApiService.GetNewMoviesAsync(page);
        if (moviesResponse == null)
        {
            return View("Error");
        }
        return View(moviesResponse);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
