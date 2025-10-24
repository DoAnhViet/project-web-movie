using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(IMovieService movieService, UserManager<ApplicationUser> userManager)
        {
            _movieService = movieService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            var users = _userManager.Users.ToList();
            
            ViewBag.TotalMovies = movies.Count;
            ViewBag.TotalUsers = users.Count;
            ViewBag.RecentMovies = movies.Take(5).ToList();
            
            return View();
        }

        // Quản lý phim
        public async Task<IActionResult> ManageMovies()
        {
            var movies = await _movieService.GetAllMoviesAsync();
            return View(movies);
        }

        [HttpGet]
        public IActionResult CreateMovie()
        {
            return View(new MovieViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovie(MovieViewModel model)
        {
            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Title = model.Title,
                    Genre = model.Genre,
                    Country = model.Country,
                    Year = model.Year,
                    Description = model.Description,
                    PosterUrl = model.PosterUrl,
                    TrailerUrl = model.TrailerUrl,
                    VideoUrl = model.VideoUrl
                };

                await _movieService.CreateMovieAsync(movie);
                TempData["SuccessMessage"] = "Phim đã được thêm thành công!";
                return RedirectToAction("ManageMovies");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditMovie(int id)
        {
            var movie = await _movieService.GetMovieByIdAsync(id);
            if (movie == null) return NotFound();

            var model = new MovieViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Genre = movie.Genre,
                Country = movie.Country,
                Year = movie.Year,
                Description = movie.Description,
                PosterUrl = movie.PosterUrl,
                TrailerUrl = movie.TrailerUrl,
                VideoUrl = movie.VideoUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditMovie(MovieViewModel model)
        {
            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Id = model.Id,
                    Title = model.Title,
                    Genre = model.Genre,
                    Country = model.Country,
                    Year = model.Year,
                    Description = model.Description,
                    PosterUrl = model.PosterUrl,
                    TrailerUrl = model.TrailerUrl,
                    VideoUrl = model.VideoUrl
                };

                await _movieService.UpdateMovieAsync(movie);
                TempData["SuccessMessage"] = "Phim đã được cập nhật thành công!";
                return RedirectToAction("ManageMovies");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var result = await _movieService.DeleteMovieAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Phim đã được xóa thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa phim!";
            }
            return RedirectToAction("ManageMovies");
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
