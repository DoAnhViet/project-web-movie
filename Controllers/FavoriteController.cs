using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using System.Security.Claims;

namespace WebMovie.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoriteController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var favorites = await _context.FavoriteMovies
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedAt)
                .Select(f => new MovieItem
                {
                    Slug = f.MovieSlug,
                    Name = f.MovieTitle,
                    OriginName = f.OriginName,
                    PosterUrl = f.PosterUrl,
                    ThumbUrl = f.ThumbUrl,
                    Year = f.Year
                })
                .ToListAsync();

            return View(favorites);
        }
    }
}