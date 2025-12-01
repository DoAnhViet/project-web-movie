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
        private readonly CommentService _commentService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            MovieApiService movieApiService,
            WatchAnalyticsService watchAnalyticsService,
            CommentService commentService)
        {
            _userManager = userManager;
            _db = db;
            _movieApiService = movieApiService;
            _watchAnalyticsService = watchAnalyticsService;
            _commentService = commentService;
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
        public async Task<IActionResult> ManageMovies(int page = 1, string? search = null, bool showHidden = false)
        {
            // Lấy danh sách phim đã bị ẩn
            var hiddenMovieSlugs = await _db.CustomMovieTitles
                .Where(c => c.IsHidden)
                .Select(c => c.MovieSlug)
                .ToListAsync();
            
            List<WebMovie.Models.MovieItem> items = new();
            int totalPages = 1;
            int totalItems = 0;
            
            // Nếu xem phim bị ẩn, cần fetch tất cả pages để tìm phim ẩn
            if (showHidden && hiddenMovieSlugs.Any())
            {
                // Fetch các trang để tìm phim ẩn
                int currentPage = 1;
                int maxPages = 10; // Giới hạn để tránh fetch quá nhiều
                var allMovies = new List<WebMovie.Models.MovieItem>();
                
                while (currentPage <= maxPages)
                {
                    var response = await _movieApiService.GetAllMoviesForAdminAsync(currentPage, 20);
                    if (response?.Items == null || !response.Items.Any())
                        break;
                        
                    allMovies.AddRange(response.Items);
                    totalItems = response?.Pagination?.TotalItems ?? 0;
                    totalPages = response?.Pagination?.TotalPages ?? 1;
                    
                    if (currentPage >= totalPages)
                        break;
                        
                    currentPage++;
                }
                
                items = allMovies.Where(m => hiddenMovieSlugs.Contains(m.Slug)).ToList();
            }
            else
            {
                // Xem phim thường: chỉ fetch một trang
                var response = await _movieApiService.GetAllMoviesForAdminAsync(page, 20);
                items = response?.Items ?? new List<WebMovie.Models.MovieItem>();
                totalPages = response?.Pagination?.TotalPages ?? 1;
                totalItems = response?.Pagination?.TotalItems ?? 0;
                
                // Mặc định: không hiển thị phim bị ẩn
                items = items.Where(m => !hiddenMovieSlugs.Contains(m.Slug)).ToList();
            }
            
            // Lọc theo tên tìm kiếm nếu có
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower().Trim();
                items = items
                    .Where(m => (m.Name ?? "").ToLower().Contains(searchLower) || 
                                (m.OriginName ?? "").ToLower().Contains(searchLower) ||
                                (m.Slug ?? "").ToLower().Contains(searchLower))
                    .ToList();
            }
            
            ViewBag.HiddenMovieSlugs = hiddenMovieSlugs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.SearchQuery = search;
            ViewBag.ShowOnlyHidden = showHidden;
            
            return View(items);
        }

        /// <summary>
        /// Tìm kiếm phim real-time (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchMoviesRealTime(string? q = null, bool showHidden = false)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { success = true, movies = new List<dynamic>() });
            }

            var searchLower = q.ToLower().Trim();

            // Lấy danh sách phim bị ẩn
            var hiddenMovieSlugs = await _db.CustomMovieTitles
                .Where(c => c.IsHidden)
                .Select(c => c.MovieSlug)
                .ToListAsync();

            // Fetch movies từ API
            var allMovies = new List<WebMovie.Models.MovieItem>();
            if (showHidden && hiddenMovieSlugs.Any())
            {
                // Fetch tất cả pages để tìm phim ẩn
                int currentPage = 1;
                while (currentPage <= 10)
                {
                    var response = await _movieApiService.GetAllMoviesForAdminAsync(currentPage, 20);
                    if (response?.Items == null || !response.Items.Any())
                        break;

                    allMovies.AddRange(response.Items);
                    if (currentPage >= (response?.Pagination?.TotalPages ?? 1))
                        break;

                    currentPage++;
                }
                allMovies = allMovies.Where(m => hiddenMovieSlugs.Contains(m.Slug)).ToList();
            }
            else
            {
                var response = await _movieApiService.GetAllMoviesForAdminAsync(1, 50);
                allMovies = response?.Items ?? new List<WebMovie.Models.MovieItem>();
                allMovies = allMovies.Where(m => !hiddenMovieSlugs.Contains(m.Slug)).ToList();
            }

            // Tìm kiếm
            var movies = allMovies
                .Where(m => (m.Name ?? "").ToLower().Contains(searchLower) ||
                           (m.OriginName ?? "").ToLower().Contains(searchLower) ||
                           (m.Slug ?? "").ToLower().Contains(searchLower))
                .Take(20)
                .Select(m => new
                {
                    slug = m.Slug,
                    name = m.Name,
                    originName = m.OriginName,
                    posterUrl = m.PosterUrl,
                    quality = m.Quality,
                    isHidden = hiddenMovieSlugs.Contains(m.Slug)
                })
                .ToList();

            return Json(new { success = true, movies = movies });
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

        /// <summary>
        /// Xem chi tiết phim từ Admin
        /// </summary>
        public async Task<IActionResult> ViewMovieDetail(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                TempData["ErrorMessage"] = "Slug phim không tìm thấy.";
                return RedirectToAction("ManageMovies");
            }

            try
            {
                var detail = await _movieApiService.GetMovieDetailAsync(slug);
                if (detail?.Movie == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin phim từ API.";
                    return RedirectToAction("ManageMovies");
                }

                // Kiểm tra nếu phim bị ẩn
                var isHidden = await _db.CustomMovieTitles.AnyAsync(c => c.MovieSlug == slug && c.IsHidden);
                ViewBag.IsHidden = isHidden;

                return View("MovieDetail", detail.Movie);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lấy thông tin phim: " + ex.Message;
                return RedirectToAction("ManageMovies");
            }
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

        // ===== QUẢN LÝ BÌNH LUẬN =====
        
        /// <summary>
        /// Hiển thị danh sách tất cả bình luận (với phân trang)
        /// </summary>
        public async Task<IActionResult> ManageComments(int page = 1, int pageSize = 20, string? filterMovie = null)
        {
            var query = _db.MovieComments
                .Include(c => c.User)
                .AsQueryable();

            // Lọc theo phim nếu có
            if (!string.IsNullOrEmpty(filterMovie))
            {
                query = query.Where(c => c.MovieTitle.Contains(filterMovie) || c.MovieSlug.Contains(filterMovie));
            }

            // Tính toán phân trang
            var totalComments = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);
            
            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalComments = totalComments;
            ViewBag.FilterMovie = filterMovie;
            ViewBag.PageSize = pageSize;

            return View(comments);
        }

        /// <summary>
        /// Xem chi tiết bình luận
        /// </summary>
        public async Task<IActionResult> CommentDetail(int id)
        {
            var comment = await _db.MovieComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bình luận!";
                return RedirectToAction("ManageComments");
            }

            return View(comment);
        }

        /// <summary>
        /// Sửa bình luận
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> EditComment(int id, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không thể trống!" });
            }

            if (content.Length > 1000)
            {
                return Json(new { success = false, message = "Nội dung bình luận không thể vượt quá 1000 ký tự!" });
            }

            var comment = await _db.MovieComments.FindAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận!" });
            }

            comment.Content = content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Bình luận đã được cập nhật!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi cập nhật: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa bình luận
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _db.MovieComments.FindAsync(id);
            if (comment == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bình luận!" });
            }

            try
            {
                _db.MovieComments.Remove(comment);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Bình luận đã được xóa!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy thống kê bình luận (dùng cho dashboard)
        /// </summary>
        public async Task<IActionResult> GetCommentStats()
        {
            var totalComments = await _db.MovieComments.CountAsync();
            var commentsToday = await _db.MovieComments
                .CountAsync(c => c.CreatedAt >= DateTime.UtcNow.Date);
            var commentsThisMonth = await _db.MovieComments
                .CountAsync(c => c.CreatedAt.Year == DateTime.UtcNow.Year 
                    && c.CreatedAt.Month == DateTime.UtcNow.Month);

            // Lấy top 10 phim có bình luận nhiều nhất
            var topMoviesWithComments = await _db.MovieComments
                .GroupBy(c => new { c.MovieSlug, c.MovieTitle })
                .Select(g => new 
                { 
                    MovieSlug = g.Key.MovieSlug,
                    MovieTitle = g.Key.MovieTitle,
                    CommentCount = g.Count()
                })
                .OrderByDescending(x => x.CommentCount)
                .Take(10)
                .ToListAsync();

            // Lấy top 10 người dùng bình luận nhiều nhất
            var topCommenters = await _db.MovieComments
                .GroupBy(c => c.UserId)
                .Select(g => new 
                { 
                    UserId = g.Key,
                    UserName = g.FirstOrDefault()!.User != null ? g.FirstOrDefault()!.User!.UserName : "Unknown",
                    CommentCount = g.Count()
                })
                .OrderByDescending(x => x.CommentCount)
                .Take(10)
                .ToListAsync();

            return Json(new 
            { 
                totalComments,
                commentsToday,
                commentsThisMonth,
                topMoviesWithComments,
                topCommenters
            });
        }

        /// <summary>
        /// Xóa tất cả bình luận của một phim
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAllCommentsForMovie(string movieSlug)
        {
            if (string.IsNullOrEmpty(movieSlug))
            {
                return Json(new { success = false, message = "Slug phim không hợp lệ!" });
            }

            try
            {
                var comments = await _db.MovieComments
                    .Where(c => c.MovieSlug == movieSlug)
                    .ToListAsync();

                if (comments.Count == 0)
                {
                    return Json(new { success = false, message = "Không có bình luận nào cho phim này!" });
                }

                _db.MovieComments.RemoveRange(comments);
                await _db.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã xóa {comments.Count} bình luận!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }

        /// <summary>
        /// Xóa tất cả bình luận của một người dùng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAllCommentsFromUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID không hợp lệ!" });
            }

            try
            {
                var comments = await _db.MovieComments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (comments.Count == 0)
                {
                    return Json(new { success = false, message = "Người dùng này không có bình luận nào!" });
                }

                _db.MovieComments.RemoveRange(comments);
                await _db.SaveChangesAsync();

                return Json(new 
                { 
                    success = true, 
                    message = $"Đã xóa {comments.Count} bình luận từ người dùng!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi xóa: {ex.Message}" });
            }
        }

        /// <summary>
        /// Tìm kiếm bình luận theo nội dung
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchComments(string query, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("ManageComments");
            }

            var searchQuery = _db.MovieComments
                .Include(c => c.User)
                .Where(c => c.Content.Contains(query) || c.MovieTitle.Contains(query))
                .AsQueryable();

            var totalComments = await searchQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalComments / (double)pageSize);

            var comments = await searchQuery
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalComments = totalComments;
            ViewBag.SearchQuery = query;
            ViewBag.PageSize = pageSize;

            return View("ManageComments", comments);
        }

        /// <summary>
        /// Tìm kiếm bình luận real-time (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchCommentsRealTime(string? q = null)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { success = true, comments = new List<dynamic>() });
            }

            var searchLower = q.ToLower().Trim();
            
            var comments = await _db.MovieComments
                .Include(c => c.User)
                .Where(c => c.Content.ToLower().Contains(searchLower) || 
                           c.MovieTitle.ToLower().Contains(searchLower) ||
                           c.MovieSlug.ToLower().Contains(searchLower))
                .OrderByDescending(c => c.CreatedAt)
                .Take(50)  // Giới hạn 50 kết quả
                .Select(c => new
                {
                    id = c.Id,
                    content = c.Content.Length > 100 ? c.Content.Substring(0, 100) + "..." : c.Content,
                    movieTitle = c.MovieTitle,
                    userName = c.User.FullName ?? c.User.UserName,
                    createdAt = c.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    userId = c.UserId,
                    movieSlug = c.MovieSlug
                })
                .ToListAsync();

            return Json(new { success = true, comments = comments });
        }
    }
}
