using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebMovie.Models;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MovieApiService _movieApiService;
        private readonly WatchAnalyticsService _watchAnalyticsService;

        public HomeController(
            ILogger<HomeController> logger,
            MovieApiService movieApiService,
            WatchAnalyticsService watchAnalyticsService)
        {
            _logger = logger;
            _movieApiService = movieApiService;
            _watchAnalyticsService = watchAnalyticsService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var viewModel = new HomeIndexViewModel();
            const int latestSectionSize = 30;

            try
            {
                viewModel.LatestMovies = await _movieApiService.GetNewMoviesAsync(page, latestSectionSize);

                if (viewModel.LatestMovies?.Items != null)
                {
                    var items = viewModel.LatestMovies.Items
                        .Where(item => item != null && !string.IsNullOrEmpty(item.Slug))
                        .GroupBy(item => item.Slug)
                        .Select(group => group.First())
                        .ToList();

                    var totalPages = viewModel.LatestMovies.Pagination?.TotalPages ?? int.MaxValue;
                    var nextPage = page + 1;
                    var safetyCounter = 3;

                    while (items.Count < latestSectionSize && safetyCounter-- > 0)
                    {
                        if (nextPage > totalPages)
                        {
                            break;
                        }

                        var extraResponse = await _movieApiService.GetNewMoviesAsync(nextPage, latestSectionSize);
                        if (extraResponse?.Items == null || !extraResponse.Items.Any())
                        {
                            break;
                        }

                        foreach (var extra in extraResponse.Items)
                        {
                            if (extra == null || string.IsNullOrEmpty(extra.Slug))
                            {
                                continue;
                            }

                            if (items.Any(existing => existing.Slug == extra.Slug))
                            {
                                continue;
                            }

                            items.Add(extra);
                            if (items.Count >= latestSectionSize)
                            {
                                break;
                            }
                        }

                        nextPage++;
                    }

                    viewModel.LatestMovies.Items = items.Take(latestSectionSize).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                ViewBag.ErrorMessage = "Không thể tải danh sách phim.";
            }

            try
            {
                var analyticsMovies = await _watchAnalyticsService.GetTopWatchedMoviesAsync(10);
                viewModel.TopWatched = analyticsMovies
                    .Select(movie => new TopWatchedMovieViewModel
                    {
                        Slug = movie.Slug,
                        Title = string.IsNullOrEmpty(movie.Title) ? movie.Slug : movie.Title,
                        OriginTitle = movie.OriginTitle,
                        PosterUrl = movie.PosterUrl,
                        Quality = movie.Quality,
                        WatchCount = movie.WatchCount
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading top watched movies");
            }

            if ((viewModel.TopWatched == null || viewModel.TopWatched.Count == 0) && viewModel.LatestMovies?.Items != null)
            {
                viewModel.TopWatched ??= new List<TopWatchedMovieViewModel>();

                foreach (var item in viewModel.LatestMovies.Items)
                {
                    if (string.IsNullOrEmpty(item.Slug))
                    {
                        continue;
                    }

                    if (viewModel.TopWatched.Any(t => t.Slug == item.Slug))
                    {
                        continue;
                    }

                    viewModel.TopWatched.Add(new TopWatchedMovieViewModel
                    {
                        Slug = item.Slug,
                        Title = item.Name ?? item.Slug,
                        OriginTitle = item.OriginName,
                        PosterUrl = item.PosterUrl,
                        Quality = item.Quality,
                        WatchCount = 0
                    });

                    if (viewModel.TopWatched.Count >= 10)
                    {
                        break;
                    }
                }
            }

            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
