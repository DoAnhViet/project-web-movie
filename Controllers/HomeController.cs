// Controllers/HomeController.cs
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

    public async Task<IActionResult> Index()
    {
        var newMovies = await _movieApiService.GetMoviesByCategoryAsync("hoat-hinh", 1);
        return View(newMovies ?? new MovieListResponse());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}