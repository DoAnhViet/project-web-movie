using Microsoft.EntityFrameworkCore;
using WebMovie.Models;

namespace WebMovie.Services
{
    public interface IMovieService
    {
        Task<List<Movie>> GetAllMoviesAsync();
        Task<Movie?> GetMovieByIdAsync(int id);
        Task<Movie> CreateMovieAsync(Movie movie);
        Task<Movie> UpdateMovieAsync(Movie movie);
        Task<bool> DeleteMovieAsync(int id);
        Task<List<Movie>> SearchMoviesAsync(string searchTerm);
        Task<List<ApiMovie>> GetApiMoviesAsync(int page = 1, int limit = 20);
        Task<ApiMovie?> GetApiMovieDetailAsync(string slug);
        Task<List<ApiMovie>> SearchApiMoviesAsync(string keyword, int page = 1, int limit = 20);
    }

    public class MovieService : IMovieService
    {
        private readonly ApplicationDbContext _context;
        private readonly IApiService _apiService;

        public MovieService(ApplicationDbContext context, IApiService apiService)
        {
            _context = context;
            _apiService = apiService;
        }

        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            return await _context.Movies.ToListAsync();
        }

        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies.FindAsync(id);
        }

        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<Movie> UpdateMovieAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
            return movie;
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Movie>> SearchMoviesAsync(string searchTerm)
        {
            return await _context.Movies
                .Where(m => m.Title.Contains(searchTerm) || 
                           m.Genre.Contains(searchTerm) || 
                           m.Description.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<List<ApiMovie>> GetApiMoviesAsync(int page = 1, int limit = 20)
        {
            return await _apiService.GetMoviesAsync(page, limit);
        }

        public async Task<ApiMovie?> GetApiMovieDetailAsync(string slug)
        {
            return await _apiService.GetMovieDetailAsync(slug);
        }

        public async Task<List<ApiMovie>> SearchApiMoviesAsync(string keyword, int page = 1, int limit = 20)
        {
            return await _apiService.SearchMoviesAsync(keyword, page, limit);
        }
    }
}
