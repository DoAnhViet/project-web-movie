using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext db, MovieApiService movieApiService)
        {
            _userManager = userManager;
            _db = db;
            _movieApiService = movieApiService;
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
            var commentsCount = await _db.Comments.CountAsync();

            // try to get total movies from API (best-effort)
            int totalMoviesFromApi = 0;
            try
            {
                var list = await _movieApiService.GetNewMoviesAsync(1);
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

            // top movies by watch count
            var top = await _db.WatchHistories
                .GroupBy(w => w.MovieSlug)
                .Select(g => new { MovieSlug = g.Key, Count = g.Count(), Title = g.Max(x => x.MovieTitle) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topMovies = new List<TopMovieViewModel>();
            foreach (var t in top)
            {
                var title = t.Title;
                if (string.IsNullOrEmpty(title))
                {
                    try
                    {
                        var detail = await _movieApiService.GetMovieDetailAsync(t.MovieSlug);
                        title = detail?.Movie?.Name ?? t.MovieSlug;
                    }
                    catch
                    {
                        title = t.MovieSlug;
                    }
                }
                topMovies.Add(new TopMovieViewModel { Slug = t.MovieSlug, Title = title, Views = t.Count });
            }

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalWatchHistory = totalWatch,
                ViewsToday = viewsToday,
                CommentsCount = commentsCount,
                TotalMoviesFromApi = totalMoviesFromApi,
                TopMovies = topMovies
            };

            return vm;
        }

        // Dashboard overview
        public async Task<IActionResult> Dashboard()
        {
            // totals
            var totalUsers = await _userManager.Users.CountAsync();
            var totalWatch = await _db.WatchHistories.CountAsync();
            var viewsToday = await _db.WatchHistories.CountAsync(w => w.LastWatchedAt >= DateTime.UtcNow.Date);
            var commentsCount = await _db.Comments.CountAsync();

            // try to get total movies from API (best-effort)
            int totalMoviesFromApi = 0;
            try
            {
                var list = await _movieApiService.GetNewMoviesAsync(1);
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

            // top movies by watch count
            var top = await _db.WatchHistories
                .GroupBy(w => w.MovieSlug)
                .Select(g => new { MovieSlug = g.Key, Count = g.Count(), Title = g.Max(x => x.MovieTitle) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var topMovies = new List<TopMovieViewModel>();
            foreach (var t in top)
            {
                var title = t.Title;
                if (string.IsNullOrEmpty(title))
                {
                    try
                    {
                        var detail = await _movieApiService.GetMovieDetailAsync(t.MovieSlug);
                        title = detail?.Movie?.Name ?? t.MovieSlug;
                    }
                    catch
                    {
                        title = t.MovieSlug;
                    }
                }
                topMovies.Add(new TopMovieViewModel { Slug = t.MovieSlug, Title = title, Views = t.Count });
            }

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalWatchHistory = totalWatch,
                ViewsToday = viewsToday,
                CommentsCount = commentsCount,
                TotalMoviesFromApi = totalMoviesFromApi,
                TopMovies = topMovies
            };

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
        public async Task<IActionResult> ManageMovies(int page = 1)
        {
            var response = await _movieApiService.GetNewMoviesAsync(page);
            var items = response?.Items ?? new List<WebMovie.Models.MovieItem>();
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
