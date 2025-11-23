using System.Collections.Generic;

namespace WebMovie.Models
{
    public class TopMovieViewModel
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Views { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalWatchHistory { get; set; }
        public int ViewsToday { get; set; }
        public int CommentsCount { get; set; }
        public int TotalMoviesFromApi { get; set; }
        public List<TopMovieViewModel> TopMovies { get; set; } = new List<TopMovieViewModel>();
    }
}
