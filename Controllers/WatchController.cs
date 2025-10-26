using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class WatchController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public WatchController(MovieApiService movieApiService, ApplicationDbContext context)
            : base(movieApiService) // ✅ Gọi base constructor
        {
            _context = context;
        }

        // Xem phim
        public async Task<IActionResult> Index(string slug, string? episode)
        {
            if (string.IsNullOrEmpty(slug))
                return RedirectToAction("NewMovies", "Movie");

            try
            {
                var movieDetail = await _movieApiService.GetMovieDetailAsync(slug);
                if (movieDetail?.Movie == null)
                    return RedirectToAction("NewMovies", "Movie");

                // Nếu user đã đăng nhập => lấy lịch sử xem
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var watchHistory = await _context.WatchHistories
                            .Where(w => w.UserId == userId && w.MovieSlug == slug)
                            .OrderByDescending(w => w.LastWatchedAt)
                            .FirstOrDefaultAsync();

                        ViewBag.WatchHistory = watchHistory;
                    }
                }

                return View(movieDetail);
            }
            catch
            {
                return RedirectToAction("NewMovies", "Movie");
            }
        }

        // API lưu tiến trình xem
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveWatchProgress([FromBody] WatchProgressRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

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

        // Lịch sử xem
        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var histories = await _context.WatchHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.LastWatchedAt)
                .Take(50)
                .ToListAsync();

            return View(histories);
        }
    }

    // Model lưu tiến trình xem
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
