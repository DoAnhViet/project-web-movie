using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly MovieApiService _movieApiService;
        private readonly WatchAnalyticsService _watchAnalyticsService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            MovieApiService movieApiService,
            WatchAnalyticsService watchAnalyticsService)
        {
            _userManager = userManager;
            _db = db;
            _movieApiService = movieApiService;
            _watchAnalyticsService = watchAnalyticsService;
        }

        public async Task<IActionResult> Index()
        {
            var vm = await BuildDashboardVmAsync();
            // render the Dashboard view inside Admin index route
            return View("Dashboard", vm);
        }

        private async Task<AdminDashboardViewModel> BuildDashboardVmAsync()
        {
            // totals
            var totalUsers = await _userManager.Users.CountAsync();
            var totalWatch = await _db.WatchHistories.CountAsync();
            var viewsToday = await _db.WatchHistories.CountAsync(w => w.LastWatchedAt >= DateTime.UtcNow.Date);
            var commentsCount = await _db.MovieComments.CountAsync();

            // try to get total movies from API (best-effort) - dùng method cho admin
            int totalMoviesFromApi = 0;
            try
            {
                var list = await _movieApiService.GetNewMoviesForAdminAsync(1);
                if (list?.Pagination != null)
                {
                    totalMoviesFromApi = list.Pagination.TotalItems;
                }
                else
                {
                    totalMoviesFromApi = list?.Items?.Count ?? 0;
                }
            }
            catch
            {
                totalMoviesFromApi = 0;
            }

            var analyticsTopMovies = await _watchAnalyticsService.GetTopWatchedMoviesAsync(10);
            var topMovies = analyticsTopMovies
                .Select(movie => new TopMovieViewModel
                {
                    Slug = movie.Slug,
                    Title = string.IsNullOrEmpty(movie.Title) ? movie.Slug : movie.Title,
                    Views = movie.WatchCount
                })
                .ToList();

            // Đếm số phim đã bị ẩn
            var hiddenMoviesCount = await _db.CustomMovieTitles.CountAsync(c => c.IsHidden);

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalWatchHistory = totalWatch,
                ViewsToday = viewsToday,
                CommentsCount = commentsCount,
                TotalMoviesFromApi = totalMoviesFromApi,
                HiddenMoviesCount = hiddenMoviesCount,
                TopMovies = topMovies
            };

            return vm;
        }

        // Dashboard overview
        public async Task<IActionResult> Dashboard()
        {
            var vm = await BuildDashboardVmAsync();
            return View(vm);
        }

        // Quản lý người dùng
        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        // ----- Custom movie title management -----
        // List custom titles
        public async Task<IActionResult> Titles()
        {
            var titles = _db.CustomMovieTitles.OrderByDescending(t => t.UpdatedAt).ToList();
            return View(titles);
        }

        // Manage movies (fetch from API and allow editing title)
        // Manage movies (fetch from API and allow editing title)
        public async Task<IActionResult> ManageMovies(int page = 1)
        {
            // Dùng method lấy tất cả phim cho admin (không filter phim đã ẩn)
            // Tăng limit lên 20 để hiển thị nhiều phim hơn
            var response = await _movieApiService.GetAllMoviesForAdminAsync(page, 20);
            var items = response?.Items ?? new List<WebMovie.Models.MovieItem>();
            
            // Lấy danh sách phim đã bị ẩn
            var hiddenMovieSlugs = await _db.CustomMovieTitles
                .Where(c => c.IsHidden)
                .Select(c => c.MovieSlug)
                .ToListAsync();
            
            ViewBag.HiddenMovieSlugs = hiddenMovieSlugs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = response?.Pagination?.TotalPages ?? 1;
            ViewBag.TotalItems = response?.Pagination?.TotalItems ?? 0;
            
            return View(items);
        }

        // Edit or create by slug
        [HttpGet]
        public async Task<IActionResult> EditTitle(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return View(new CustomMovieTitle());
            }

            var model = _db.CustomMovieTitles.FirstOrDefault(c => c.MovieSlug == slug);
            if (model == null)
            {
                // create new with slug prefilled and try to fetch original title from API
                model = new CustomMovieTitle { MovieSlug = slug ?? string.Empty };
                try
                {
                    var detail = await _movieApiService.GetMovieDetailAsync(slug!);
                    if (detail?.Movie != null)
                    {
                        model.OriginalTitle = detail.Movie.Name ?? string.Empty;
                    }
                }
                catch
                {
                    // ignore API errors here; admin can fill OriginalTitle manually
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTitle(CustomMovieTitle model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = _db.CustomMovieTitles.FirstOrDefault(c => c.MovieSlug == model.MovieSlug);
            if (existing == null)
            {
                // If admin didn't provide OriginalTitle, try to fetch from API
                if (string.IsNullOrEmpty(model.OriginalTitle))
                {
                    try
                    {
                        var detail = await _movieApiService.GetMovieDetailAsync(model.MovieSlug);
                        if (detail?.Movie != null)
                        {
                            model.OriginalTitle = detail.Movie.Name ?? string.Empty;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }

                model.UpdatedAt = DateTime.UtcNow;
                model.UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                _db.CustomMovieTitles.Add(model);
            }
            else
            {
                existing.CustomTitle = model.CustomTitle;
                existing.CustomDescription = model.CustomDescription;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tiêu đề đã được lưu.";
            // After saving go back to ManageMovies so admin sees the list (and API pages will reflect override)
            return RedirectToAction("ManageMovies");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTitle(int id)
        {
            var item = await _db.CustomMovieTitles.FindAsync(id);
            if (item == null) return NotFound();

            _db.CustomMovieTitles.Remove(item);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tiêu đề đã được xóa.";
            return RedirectToAction("Titles");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleHideMovie(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return Json(new { success = false, message = "Slug phim không hợp lệ." });
            }

            var customTitle = await _db.CustomMovieTitles.FirstOrDefaultAsync(c => c.MovieSlug == slug);
            
            if (customTitle == null)
            {
                // Nếu chưa có record, tạo mới với IsHidden = true
                try
                {
                    var detail = await _movieApiService.GetMovieDetailAsync(slug);
                    customTitle = new CustomMovieTitle
                    {
                        MovieSlug = slug,
                        CustomTitle = detail?.Movie?.Name ?? slug,
                        OriginalTitle = detail?.Movie?.Name ?? string.Empty,
                        IsHidden = true,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    };
                    _db.CustomMovieTitles.Add(customTitle);
                }
                catch
                {
                    // Nếu không lấy được từ API, tạo với thông tin tối thiểu
                    customTitle = new CustomMovieTitle
                    {
                        MovieSlug = slug,
                        CustomTitle = slug,
                        OriginalTitle = string.Empty,
                        IsHidden = true,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    };
                    _db.CustomMovieTitles.Add(customTitle);
                }
            }
            else
            {
                // Toggle trạng thái ẩn/hiện
                customTitle.IsHidden = !customTitle.IsHidden;
                customTitle.UpdatedAt = DateTime.UtcNow;
                customTitle.UpdatedBy = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            await _db.SaveChangesAsync();
            
            return Json(new 
            { 
                success = true, 
                isHidden = customTitle.IsHidden,
                message = customTitle.IsHidden ? "Đã ẩn phim khỏi website." : "Đã hiển thị phim trên website."
            });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var isInRole = await _userManager.IsInRoleAsync(user, role);
            
            if (isInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Đã xóa quyền {role} khỏi người dùng!";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Đã thêm quyền {role} cho người dùng!";
            }

            return RedirectToAction("ManageUsers");
        }

        // CRUD User Operations
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View(new CreateUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Gán role mặc định
                    await _userManager.AddToRoleAsync(user, model.Role);
                    TempData["SuccessMessage"] = "Người dùng đã được tạo thành công!";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "User"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;
                user.FullName = model.FullName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Cập nhật role
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["SuccessMessage"] = "Người dùng đã được cập nhật thành công!";
                    return RedirectToAction("ManageUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho phép xóa admin chính
            if (user.Email == "admin@webmovie.com")
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản admin chính!";
                return RedirectToAction("ManageUsers");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Người dùng đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng!";
            }

            return RedirectToAction("ManageUsers");
        }
    }
}
