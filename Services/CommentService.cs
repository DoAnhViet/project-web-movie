using Microsoft.EntityFrameworkCore;
using WebMovie.Models;

namespace WebMovie.Services
{
    public class CommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả comment của 1 phim
        public async Task<List<MovieComment>> GetCommentsByMovieAsync(string movieSlug)
        {
            return await _context.MovieComments
                .Include(c => c.User)
                .Where(c => c.MovieSlug == movieSlug)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Thêm comment mới
        public async Task<bool> AddCommentAsync(string userId, string movieSlug, string movieTitle, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
                {
                    return false;
                }

                var comment = new MovieComment
                {
                    UserId = userId,
                    MovieSlug = movieSlug,
                    MovieTitle = movieTitle,
                    Content = content.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.MovieComments.Add(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommentService] Add error: {ex.Message}");
                return false;
            }
        }

        // Xóa comment (chỉ người tạo hoặc admin)
        public async Task<bool> DeleteCommentAsync(int commentId, string userId, bool isAdmin = false)
        {
            try
            {
                var comment = await _context.MovieComments.FindAsync(commentId);
                if (comment == null) return false;

                // Chỉ cho phép xóa nếu là người tạo hoặc admin
                if (comment.UserId != userId && !isAdmin)
                {
                    return false;
                }

                _context.MovieComments.Remove(comment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommentService] Delete error: {ex.Message}");
                return false;
            }
        }

        // Sửa comment
        public async Task<bool> UpdateCommentAsync(int commentId, string userId, string newContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newContent) || newContent.Length > 1000)
                {
                    return false;
                }

                var comment = await _context.MovieComments.FindAsync(commentId);
                if (comment == null || comment.UserId != userId)
                {
                    return false;
                }

                comment.Content = newContent.Trim();
                comment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CommentService] Update error: {ex.Message}");
                return false;
            }
        }

        // Đếm số comment của phim
        public async Task<int> GetCommentCountAsync(string movieSlug)
        {
            return await _context.MovieComments
                .CountAsync(c => c.MovieSlug == movieSlug);
        }
    }
}
