using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using WebMovie.Services;
using System.Security.Claims;

namespace WebMovie.Controllers
{
    public class WatchController : Controller
    {
        private readonly MovieApiService _movieApiService;
        private readonly ApplicationDbContext _context;
        private readonly FavoriteService _favoriteService; // THÊM DÒNG NÀY

        public WatchController(MovieApiService movieApiService, ApplicationDbContext context, FavoriteService favoriteService)
        {
            _movieApiService = movieApiService;
            _context = context;
            _favoriteService = favoriteService; // THÊM DÒNG NÀY
        }
                public class ToggleFavoriteRequest
        {
            public string Slug { get; set; } = "";
            public string Name { get; set; } = "";
            public string PosterUrl { get; set; } = "";
        }

        // Xem phim với episode cụ thể
        public async Task<IActionResult> Index(string slug, string? episode)
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
                    return RedirectToAction("NewMovies", "Movie");
                }

                var movie = movieDetail.Movie;

                // TRUYỀN DỮ LIỆU CHO VIEW
                ViewBag.Slug = movie.Slug;
                ViewBag.Name = movie.Name;
                ViewBag.PosterUrl = movie.PosterUrl;

                // KIỂM TRA YÊU THÍCH
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                    ViewBag.IsFavorite = await _favoriteService.IsFavoriteAsync(userId, movie.Slug);

                    // Lịch sử xem
                    var watchHistory = await _context.WatchHistories
                        .Where(w => w.UserId == userId && w.MovieSlug == slug)
                        .OrderByDescending(w => w.LastWatchedAt)
                        .FirstOrDefaultAsync();
                    ViewBag.WatchHistory = watchHistory;
                }
                else
                {
                    ViewBag.IsFavorite = false;
                }

                return View(movieDetail);
            }
            catch
            {
                return RedirectToAction("NewMovies", "Movie");
            }
        }

        // === THÊM: TOGGLE YÊU THÍCH ===
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "Không tìm thấy user" });

                if (string.IsNullOrEmpty(request.Slug))
                    return Json(new { success = false, message = "Thiếu slug phim" });

                var isFavorite = await _favoriteService.IsFavoriteAsync(userId, request.Slug);

                bool success;
                string message;

                if (isFavorite)
                {
                    success = await _favoriteService.RemoveFavoriteAsync(userId, request.Slug);
                    message = success ? "Đã xóa khỏi yêu thích!" : "Lỗi khi xóa!";
                }
                else
                {
                    var movieItem = new MovieItem
                    {
                        Slug = request.Slug,
                        Name = request.Name,
                        PosterUrl = request.PosterUrl,
                        OriginName = request.Name,
                        ThumbUrl = request.PosterUrl,
                        Year = null
                    };
                    success = await _favoriteService.AddFavoriteAsync(userId, movieItem);
                    message = success ? "Đã thêm vào yêu thích!" : "Lỗi khi thêm!";
                }

                return Json(new { success, isFavorite = !isFavorite, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ToggleFavorite] Lỗi: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = "Lỗi server!" });
            }
        }
        // Các API cũ giữ nguyên
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveWatchProgress([FromBody] WatchProgressRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var watchHistory = await _context.WatchHistories
                    .FirstOrDefaultAsync(w => 
                        w.UserId == userId && 
                        w.MovieSlug == request.MovieSlug && 
                        w.EpisodeSlug == request.EpisodeSlug);

                if (watchHistory == null)
                {
                    watchHistory = new WatchHistory
                    {
                        UserId = userId,
                        MovieSlug = request.MovieSlug,
                        MovieTitle = request.MovieTitle,
                        PosterUrl = request.PosterUrl ?? "",
                        EpisodeName = request.EpisodeName,
                        EpisodeSlug = request.EpisodeSlug,
                        CurrentTime = request.CurrentTime,
                        TotalTime = request.TotalTime,
                        FirstWatchedAt = DateTime.UtcNow,
                        LastWatchedAt = DateTime.UtcNow
                    };
                    _context.WatchHistories.Add(watchHistory);
                }
                else
                {
                    watchHistory.CurrentTime = request.CurrentTime;
                    watchHistory.TotalTime = request.TotalTime;
                    watchHistory.LastWatchedAt = DateTime.UtcNow;
                    _context.WatchHistories.Update(watchHistory);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã lưu tiến trình xem" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWatchProgress(string movieSlug, string episodeSlug)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var watchHistory = await _context.WatchHistories
                .FirstOrDefaultAsync(w => 
                    w.UserId == userId && 
                    w.MovieSlug == movieSlug && 
                    w.EpisodeSlug == episodeSlug);

            if (watchHistory == null)
                return Json(new { success = false, currentTime = 0 });

            return Json(new 
            { 
                success = true, 
                currentTime = watchHistory.CurrentTime,
                totalTime = watchHistory.TotalTime,
                progressPercent = watchHistory.ProgressPercent
            });
        }

        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var histories = await _context.WatchHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.LastWatchedAt)
                .Take(50)
                .ToListAsync();

            return View(histories);
        }
    }

    public class WatchProgressRequest
    {
        public string MovieSlug { get; set; } = "";
        public string MovieTitle { get; set; } = "";
        public string? PosterUrl { get; set; }
        public string EpisodeName { get; set; } = "";
        public string EpisodeSlug { get; set; } = "";
        public int CurrentTime { get; set; }
        public int TotalTime { get; set; }
    }
}
