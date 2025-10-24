using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class WatchController : Controller
    {
        private readonly MovieApiService _movieApiService;

        public WatchController(MovieApiService movieApiService)
        {
            _movieApiService = movieApiService;
        }

        public async Task<IActionResult> Index(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return RedirectToAction("Index", "Home");
            }

            var movieDetail = await _movieApiService.GetMovieDetailAsync(slug);
            
            if (movieDetail == null || movieDetail.Movie == null)
            {
                return NotFound();
            }

            return View(movieDetail);
        }
    }
}
