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

    public async Task<IActionResult> Index(int page)
    {

        try
        {
            var moviesResponse = await _movieApiService.GetNewMoviesAsync(page);
            return View(moviesResponse);
        }
        catch
        {
            // Nếu lỗi khi gọi API, trả về view rỗng để không crash
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("NewMovies", "Movie");
            }

            try
            {
                var movieDetail = await _movieApiService.GetMovieDetailAsync(slug);
                
                if (movieDetail == null || movieDetail.Movie == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy phim này!";
                    return RedirectToAction("NewMovies", "Movie");
                }

                return View(movieDetail);
            }
            catch
            {
                return RedirectToAction("NewMovies", "Movie");
            }
        }

}
