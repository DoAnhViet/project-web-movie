using System.Collections.Generic;

namespace WebMovie.Models
{
    public class TopWatchedMovieViewModel
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? OriginTitle { get; set; }
        public string? PosterUrl { get; set; }
        public string? Quality { get; set; }
        public int WatchCount { get; set; }
    }

    public class HomeIndexViewModel
    {
        public MovieListResponse? LatestMovies { get; set; }
        public List<TopWatchedMovieViewModel> TopWatched { get; set; } = new();
    }
}
