using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class WatchController : Controller
    {
        private readonly MovieApiService _movieApiService;
        private readonly ApplicationDbContext _context;

        public WatchController(MovieApiService movieApiService, ApplicationDbContext context)
        {
            _movieApiService = movieApiService;
            _context = context;
        }

        // Xem phim với episode cụ thể - KHÔNG YÊU CẦU ĐĂNG NHẬP
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

                // Nếu user đã login, lấy lịch sử xem
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

        // API để lưu/cập nhật lịch sử xem
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveWatchProgress([FromBody] WatchProgressRequest request)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            try
            {
                var watchHistory = await _context.WatchHistories
                    .FirstOrDefaultAsync(w => 
                        w.UserId == userId && 
                        w.MovieSlug == request.MovieSlug && 
                        w.EpisodeSlug == request.EpisodeSlug);

                if (watchHistory == null)
                {
                    // Tạo mới
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
                    // Cập nhật
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

        // API để lấy lịch sử xem của một phim
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWatchProgress(string movieSlug, string episodeSlug)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var watchHistory = await _context.WatchHistories
                .FirstOrDefaultAsync(w => 
                    w.UserId == userId && 
                    w.MovieSlug == movieSlug && 
                    w.EpisodeSlug == episodeSlug);

            if (watchHistory == null)
            {
                return Json(new { success = false, currentTime = 0 });
            }

            return Json(new 
            { 
                success = true, 
                currentTime = watchHistory.CurrentTime,
                totalTime = watchHistory.TotalTime,
                progressPercent = watchHistory.ProgressPercent
            });
        }

        // API để lấy danh sách lịch sử xem của user
        [Authorize]
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var histories = await _context.WatchHistories
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.LastWatchedAt)
                .Take(50)
                .ToListAsync();

            return View(histories);
        }
    }

    // Request model cho việc lưu tiến trình xem
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
