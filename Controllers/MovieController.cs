using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieApiService _movieApiService;

        public MovieController(MovieApiService movieApiService)
        {
            _movieApiService = movieApiService;
        }

        // Trang chi tiết phim - KHÔNG YÊU CẦU ĐĂNG NHẬP
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

        // Danh sách phim mới cập nhật
        public async Task<IActionResult> NewMovies(int page = 1)
        {
            var moviesResponse = await _movieApiService.GetNewMoviesAsync(page);
            if (moviesResponse == null)
            {
                return View("Error");
            }
            return View(moviesResponse);
        }

        // Danh sách phim theo thể loại
        public async Task<IActionResult> Category(string slug, int page = 1)
        {
            var moviesResponse = await _movieApiService.GetMoviesByCategoryAsync(slug, page);
            if (moviesResponse == null)
            {
                return View("Error");
            }
            ViewBag.CategorySlug = slug;
            return View("NewMovies", moviesResponse);
        }

        // Danh sách phim theo quốc gia
        public async Task<IActionResult> Country(string slug, int page = 1)
        {
            var moviesResponse = await _movieApiService.GetMoviesByCountryAsync(slug, page);
            if (moviesResponse == null)
            {
                return View("Error");
            }
            ViewBag.CountrySlug = slug;
            return View("NewMovies", moviesResponse);
        }

        // Tìm kiếm phim
        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index", "Home");
            }

            var moviesResponse = await _movieApiService.SearchMoviesAsync(keyword, page);
            if (moviesResponse == null)
            {
                return View("Error");
            }
            ViewBag.Keyword = keyword;
            return View("NewMovies", moviesResponse);
        }
    }
}
