using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;
using WebMovie.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebMovie.Controllers
{
    public class MovieController : Controller
    {
        private readonly MovieApiService _movieApiService;
        private readonly FavoriteService _favoriteService;

        public MovieController(MovieApiService movieApiService, FavoriteService favoriteService)
        {
            _movieApiService = movieApiService;
            _favoriteService = favoriteService;
        }


        // Trang chi tiết phim
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
                TempData["ErrorMessage"] = "Vui lòng nhập từ khóa tìm kiếm!";
                return RedirectToAction("Index", "Home");
            }

            var moviesResponse = await _movieApiService.SearchMoviesAsync(keyword, page);

            if (moviesResponse == null || !moviesResponse.Status)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phim nào phù hợp.";
                return View("NewMovies", new MovieListResponse());
            }

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = moviesResponse.Pagination?.TotalPages ?? 1;

            return View("NewMovies", moviesResponse);
        }

        // ACTION YÊU THÍCH PHIM
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(string slug, string name, string posterUrl)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var isFavorite = await _favoriteService.IsFavoriteAsync(userId, slug);

            bool success;
            string message;

            if (isFavorite)
            {
                success = await _favoriteService.RemoveFavoriteAsync(userId, slug);
                message = "Đã xóa khỏi yêu thích!";
            }
            else
            {
                var movie = new MovieItem
                {
                    Slug = slug,
                    Name = name,
                    PosterUrl = posterUrl,
                    OriginName = name,
                    ThumbUrl = posterUrl
                };
                success = await _favoriteService.AddFavoriteAsync(userId, movie);
                message = "Đã thêm vào yêu thích!";
            }

            return Json(new { success, isFavorite = !isFavorite, message });
        }
    }
}