using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebMovie.Models;

namespace WebMovie.Services
{
    public class WatchAnalyticsMovie
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? OriginTitle { get; set; }
        public string? PosterUrl { get; set; }
        public string? Quality { get; set; }
        public int WatchCount { get; set; }
    }

    public class WatchAnalyticsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly MovieApiService _movieApiService;
        private readonly ILogger<WatchAnalyticsService> _logger;

        public WatchAnalyticsService(
            ApplicationDbContext dbContext,
            MovieApiService movieApiService,
            ILogger<WatchAnalyticsService> logger)
        {
            _dbContext = dbContext;
            _movieApiService = movieApiService;
            _logger = logger;
        }

        public async Task<List<WatchAnalyticsMovie>> GetTopWatchedMoviesAsync(int count)
        {
            if (count <= 0)
            {
                return new List<WatchAnalyticsMovie>();
            }

            var grouped = await _dbContext.WatchHistories
                .Where(w => !string.IsNullOrEmpty(w.MovieSlug))
                .GroupBy(w => w.MovieSlug)
                .Select(g => new
                {
                    Slug = g.Key,
                    Watchers = g.Count(),
                    LatestInteraction = g.Max(x => x.LastWatchedAt)
                })
                .OrderByDescending(x => x.Watchers)
                .ThenByDescending(x => x.LatestInteraction)
                .Take(count)
                .ToListAsync();

            if (!grouped.Any())
            {
                return new List<WatchAnalyticsMovie>();
            }

            var slugs = grouped.Select(g => g.Slug).ToList();

            var detailLookups = await _dbContext.WatchHistories
                .Where(w => slugs.Contains(w.MovieSlug))
                .GroupBy(w => w.MovieSlug)
                .Select(g => new
                {
                    Slug = g.Key,
                    Title = g.OrderByDescending(x => x.LastWatchedAt).Select(x => x.MovieTitle).FirstOrDefault(),
                    Poster = g.OrderByDescending(x => x.LastWatchedAt).Select(x => x.PosterUrl).FirstOrDefault()
                })
                .ToListAsync();

            var result = grouped
                .Select(g =>
                {
                    var detail = detailLookups.FirstOrDefault(d => d.Slug == g.Slug);
                    return new WatchAnalyticsMovie
                    {
                        Slug = g.Slug,
                        WatchCount = g.Watchers,
                        Title = detail?.Title ?? g.Slug,
                        PosterUrl = detail?.Poster
                    };
                })
                .ToList();

            foreach (var item in result.Where(r => string.IsNullOrEmpty(r.PosterUrl) || string.IsNullOrEmpty(r.Title)))
            {
                try
                {
                    var movieDetail = await _movieApiService.GetMovieDetailAsync(item.Slug);
                    if (movieDetail?.Movie != null)
                    {
                        if (string.IsNullOrEmpty(item.Title))
                        {
                            item.Title = movieDetail.Movie.Name ?? item.Slug;
                        }
                        item.OriginTitle = movieDetail.Movie.OriginName;
                        item.PosterUrl ??= movieDetail.Movie.PosterUrl;
                        item.Quality = movieDetail.Movie.Quality;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to enrich movie detail for slug {Slug}", item.Slug);
                }
            }

            return result;
        }
    }
}
