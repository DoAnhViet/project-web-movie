// Services/FavoriteService.cs
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;

namespace WebMovie.Services
{
    public class FavoriteService
    {
        private readonly ApplicationDbContext _context;

        public FavoriteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFavoriteAsync(string userId, string movieSlug)
        {
            return await _context.FavoriteMovies
                .AnyAsync(f => f.UserId == userId && f.MovieSlug == movieSlug);
        }

        public async Task<bool> AddFavoriteAsync(string userId, MovieItem movie)
        {
            try
            {
                Console.WriteLine($"[AddFavorite] UserId: {userId}, Slug: {movie.Slug}");

                var exists = await _context.FavoriteMovies
                    .AnyAsync(f => f.UserId == userId && f.MovieSlug == movie.Slug);

                if (exists)
                {
                    Console.WriteLine("[AddFavorite] Đã tồn tại");
                    return true;
                }

                var favorite = new FavoriteMovie
                {
                    UserId = userId,
                    MovieSlug = movie.Slug,
                    MovieTitle = movie.Name,
                    OriginName = movie.OriginName ?? movie.Name,
                    PosterUrl = movie.PosterUrl,
                    ThumbUrl = movie.ThumbUrl,
                    Year = movie.Year,
                    AddedAt = DateTime.UtcNow
                };

                _context.FavoriteMovies.Add(favorite);
                var saved = await _context.SaveChangesAsync();
                Console.WriteLine($"[AddFavorite] Saved: {saved} rows");

                return saved > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FavoriteService] Add error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> RemoveFavoriteAsync(string userId, string movieSlug)
        {
            try
            {
                var favorite = await _context.FavoriteMovies
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieSlug == movieSlug);

                if (favorite == null) return true;

                _context.FavoriteMovies.Remove(favorite);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
            Console.WriteLine($"[FavoriteService] Remove error: {ex.Message}");
                return false;
            }
        }
    }
}