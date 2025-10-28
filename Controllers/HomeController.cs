using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
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
            try
            {
                var moviesResponse = await _movieApiService.GetNewMoviesAsync(page);
                return View(moviesResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                ViewBag.ErrorMessage = "Không thể tải danh sách phim.";
                return View(new MovieListResponse());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
