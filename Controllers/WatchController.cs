using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class WatchController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WatchController> _logger;

        public WatchController(MovieApiService movieApiService, ApplicationDbContext context, ILogger<WatchController> logger)
            : base(movieApiService)
        {
            _context = context;
            _logger = logger;
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
            {
                _logger?.LogWarning("SaveWatchProgress called without authenticated user.");
                return Unauthorized();
            }

            // Log incoming request for debugging
            try
            {
                _logger?.LogInformation("SaveWatchProgress request received for User={UserId}, MovieSlug={MovieSlug}, EpisodeSlug={EpisodeSlug}, CurrentTime={CurrentTime}, TotalTime={TotalTime}",
                    userId, request.MovieSlug, request.EpisodeSlug, request.CurrentTime, request.TotalTime);

                // Find existing history for this user + movie (regardless of episode)
// so we update the same movie entry instead of creating duplicates for different episodes.
                var watchHistory = await _context.WatchHistories
                    .FirstOrDefaultAsync(w =>
                        w.UserId == userId &&
                        w.MovieSlug == request.MovieSlug);

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
                    _logger?.LogInformation("Creating new WatchHistory for user {UserId}, movie {MovieSlug}", userId, request.MovieSlug);
                }
                else
                {
                    // Update movie-level fields so the existing history reflects the latest episode the user watched
                    watchHistory.MovieTitle = request.MovieTitle ?? watchHistory.MovieTitle;
                    watchHistory.PosterUrl = request.PosterUrl ?? watchHistory.PosterUrl;
                    watchHistory.EpisodeName = request.EpisodeName;
                    watchHistory.EpisodeSlug = request.EpisodeSlug;
                    watchHistory.CurrentTime = request.CurrentTime;
                    watchHistory.TotalTime = request.TotalTime;
                    watchHistory.LastWatchedAt = DateTime.UtcNow;
                    _context.WatchHistories.Update(watchHistory);
                    _logger?.LogInformation("Updated WatchHistory(Id={Id}) for user {UserId} with episode {EpisodeSlug}", watchHistory.Id, userId, request.EpisodeSlug);
                }

                var saved = await _context.SaveChangesAsync();
                _logger?.LogInformation("SaveWatchProgress completed. SaveChanges affected {Count} rows.", saved);

                return Json(new { success = true, message = "Đã lưu tiến trình xem" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in SaveWatchProgress for user {UserId}, movie {MovieSlug}", userId, request?.MovieSlug);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Lấy tiến trình xem đã lưu cho một phim (nếu có)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWatchProgress(string movieSlug)
        {
var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(movieSlug))
                return Json(new { success = false, message = "Invalid request" });

            try
            {
                var watch = await _context.WatchHistories
                    .Where(w => w.UserId == userId && w.MovieSlug == movieSlug)
                    .OrderByDescending(w => w.LastWatchedAt)
                    .FirstOrDefaultAsync();

                if (watch == null)
                    return Json(new { success = false });

                return Json(new
                {
                    success = true,
                    currentTime = watch.CurrentTime,
                    totalTime = watch.TotalTime,
                    episodeSlug = watch.EpisodeSlug,
                    episodeName = watch.EpisodeName,
                    progressPercent = watch.ProgressPercent
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error in GetWatchProgress for user {UserId}, movie {MovieSlug}", userId, movieSlug);
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

        // Xóa một mục lịch sử
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteHistory([FromBody] DeleteHistoryRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var history = await _context.WatchHistories
                    .FirstOrDefaultAsync(w => w.Id == request.Id && w.UserId == userId);

                if (history == null)
                    return Json(new { success = false, message = "Không tìm thấy lịch sử" });

                _context.WatchHistories.Remove(history);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Debug: trả về số lượng và vài mục lịch sử của user hiện tại (tạm thời)
        [Authorize]
[HttpGet]
        public async Task<IActionResult> DebugHistories()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var list = await _context.WatchHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.LastWatchedAt)
                .Take(20)
                .ToListAsync();

            return Json(new
            {
                count = list.Count,
                items = list.Select(w => new {
                    w.Id,
                    w.MovieSlug,
                    w.MovieTitle,
                    w.EpisodeName,
                    w.CurrentTime,
                    w.TotalTime,
                    w.LastWatchedAt
                })
            });
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

    // Request model for deleting history
    public class DeleteHistoryRequest
    {
        public int Id { get; set; }
    }
}