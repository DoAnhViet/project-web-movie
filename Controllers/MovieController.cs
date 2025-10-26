using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class MovieController : BaseController
    {
        public MovieController(MovieApiService movieApiService)
            : base(movieApiService)
        {
        }

        // Trang chi tiết phim
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return RedirectToAction("NewMovies");

            try
            {
                var movieDetail = await _movieApiService.GetMovieDetailAsync(slug);
                if (movieDetail?.Movie == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy phim này!";
                    return RedirectToAction("NewMovies");
                }

                return View(movieDetail);
            }
            catch
            {
                return RedirectToAction("NewMovies");
            }
        }

        // Danh sách phim mới
        public async Task<IActionResult> NewMovies(int page = 1)
        {
            var moviesResponse = await _movieApiService.GetNewMoviesAsync(page);
            return moviesResponse == null ? View("Error") : View(moviesResponse);
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
            var moviesResponse = await _movieApiService.GetMoviesByCountryAsync(slug, page);
            if (moviesResponse == null)
                return View("Error");

            ViewBag.CountrySlug = slug;
            ViewData["Title"] = $"Phim quốc gia: {slug}";
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
                year, page, sort_field, sort_type, sort_lang, category, country, limit);

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
    }
}
