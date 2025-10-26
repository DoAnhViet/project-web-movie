using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, MovieApiService movieApiService)
            : base(movieApiService)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách phim mới cập nhật
                var moviesResponse = await _movieApiService.GetNewMoviesAsync();

                // Nếu API trả về null thì tránh crash
                var allMovies = moviesResponse?.Items ?? new List<MovieItem>();
                var top5 = allMovies.Take(5).ToList();

                // Tạo ViewModel để truyền 2 danh sách ra view
                var model = new HomeViewModel
                {
                    TopMovies = top5,
                    AllMovies = allMovies
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page movies");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải dữ liệu.";
                return View(new HomeViewModel());
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
