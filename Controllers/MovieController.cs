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
            var moviesResponse = await _movieApiService.GetNewMoviesAsync(page, 12);
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
                return View("Error");

            ViewBag.CategorySlug = slug;
            ViewData["Title"] = $"Thể loại: {slug}";
            return View("Category", moviesResponse);
        }

        [HttpGet("the-loai/{slug}")]
        public async Task<IActionResult> CategoryDetail(
            string slug,
            int page = 1,
            string sort_field = "_id",
            string sort_type = "asc",
            string sort_lang = "",
            string country = "",
            string year = "",
            int limit = 20)
        {
            var moviesResponse = await _movieApiService.GetCategoryDetailAsync(
                slug, page, sort_field, sort_type, sort_lang, country, year, limit);

            if (moviesResponse == null)
                return View("Error");

            ViewData["Title"] = $"Thể loại: {slug}";
            ViewBag.CategorySlug = slug;
            ViewBag.Page = page;
            ViewBag.SortField = sort_field;
            ViewBag.SortType = sort_type;
            ViewBag.SortLang = sort_lang;
            ViewBag.Country = country;
            ViewBag.Year = year;
            ViewBag.Limit = limit;

            return View("Category", moviesResponse);
        }

        // Danh sách phim theo quốc gia
        public async Task<IActionResult> Country(string slug, int page = 1)
        {
            if (string.IsNullOrEmpty(slug))
                return RedirectToAction("NewMovies");

            var moviesResponse = await _movieApiService.GetMoviesByCountryAsync(slug, page);

            moviesResponse ??= new MovieListResponse
            {
                Items = new List<MovieItem>(),
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    TotalPages = 1,
                    TotalItems = 0,
                    TotalItemsPerPage = 20
                }
            };
            var countryName = slug switch
            {
                "han-quoc" => "Hàn Quốc",
                "trung-quoc" => "Trung Quốc",
                "my" => "Mỹ",
                "nhat-ban" => "Nhật Bản",
                "thai-lan" => "Thái Lan",
                "hong-kong" => "Hồng Kông",
                "dai-loan" => "Đài Loan",
                "viet-nam" => "Việt Nam",
                "an-do" => "Ấn Độ",
                _ => slug.Replace("-", " ").ToUpperFirst() // ví dụ: thai-lan → Thai Lan
            };

    ViewBag.CountryName = countryName;

            ViewBag.CountrySlug = slug;
            ViewBag.Page = page;
            ViewData["Title"] = $"Phim {slug.Replace("-", " ")}";   

            return View("NewMovies", moviesResponse);
        }
        // 🔍 Tìm kiếm phim
        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction("Index", "Home");

            var moviesResponse = await _movieApiService.SearchMoviesAsync(keyword, page);
            if (moviesResponse == null)
                return View("Error");

            ViewBag.Keyword = keyword;
            ViewData["Title"] = $"Kết quả tìm kiếm: {keyword}";
            return View("NewMovies", moviesResponse);
        }

        // 🗓️ Lọc phim theo năm phát hành
        [HttpGet("nam/{year}")]
        public async Task<IActionResult> ByYear(
            int year,
            int page = 1,
            string sort_field = "_id",
            string sort_type = "asc",
            string sort_lang = "",
            string category = "",
            string country = "",
            int limit = 20)
        {
            var moviesResponse = await _movieApiService.GetMoviesByYearAsync(
                year.ToString(), page, sort_field, sort_type, sort_lang, category, country, limit);

            if (moviesResponse == null)
                return View("Error");

            ViewData["Title"] = $"Phim năm {year}";
            ViewBag.Year = year;
            ViewBag.Page = page;
            ViewBag.SortField = sort_field;
            ViewBag.SortType = sort_type;
            ViewBag.SortLang = sort_lang;
            ViewBag.Category = category;
            ViewBag.Country = country;
            ViewBag.Limit = limit;

            // Dùng lại view hiển thị danh sách phim
            return View("NewMovies", moviesResponse);
        }

        // ACTION YÊU THÍCH PHIM
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(string slug, string name, string posterUrl)
        {
            // Validate parameters
            if (string.IsNullOrWhiteSpace(slug))
            {
                return Json(new { success = false, message = "Thông tin phim không hợp lệ (slug)" });
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Thông tin phim không hợp lệ (name)" });
            }

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
                    PosterUrl = posterUrl ?? "",
                    OriginName = name,
                    ThumbUrl = posterUrl ?? ""
                };
                success = await _favoriteService.AddFavoriteAsync(userId, movie);
                message = "Đã thêm vào yêu thích!";
            }

            return Json(new { success, isFavorite = !isFavorite, message });
        }
        
    }
    public static class StringExtensions
    {
        public static string ToUpperFirst(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpper();
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}