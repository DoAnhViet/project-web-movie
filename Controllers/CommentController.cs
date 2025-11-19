using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly CommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(CommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        // POST: Thêm comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string movieSlug, string movieTitle, string content)
        {
            _logger.LogInformation("Comment Add called - MovieSlug: {MovieSlug}, Content length: {Length}", movieSlug, content?.Length ?? 0);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Comment content is empty");
                return Json(new { success = false, message = "Nội dung comment không được để trống" });
            }

            if (content.Length > 1000)
            {
                _logger.LogWarning("Comment too long: {Length} characters", content.Length);
                return Json(new { success = false, message = "Comment không được quá 1000 ký tự" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("User ID from claims: {UserId}", userId ?? "NULL");
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User not authenticated");
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            try
            {
                var success = await _commentService.AddCommentAsync(userId, movieSlug, movieTitle, content);

                if (success)
                {
                    _logger.LogInformation("Comment added successfully for user {UserId}", userId);
                    return Json(new { success = true, message = "Đã thêm comment thành công!" });
                }

                _logger.LogWarning("Failed to add comment for user {UserId}", userId);
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception when adding comment for user {UserId}", userId);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Xóa comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int commentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var isAdmin = User.IsInRole("Admin");
            var success = await _commentService.DeleteCommentAsync(commentId, userId, isAdmin);

            if (success)
            {
                return Json(new { success = true, message = "Đã xóa comment" });
            }

            return Json(new { success = false, message = "Không thể xóa comment này" });
        }

        // POST: Sửa comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int commentId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung không được để trống" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var success = await _commentService.UpdateCommentAsync(commentId, userId, content);

            if (success)
            {
                return Json(new { success = true, message = "Đã cập nhật comment", newContent = content });
            }

            return Json(new { success = false, message = "Không thể sửa comment này" });
        }

        // GET: Lấy danh sách comment
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(string movieSlug)
        {
            if (string.IsNullOrEmpty(movieSlug))
            {
                return Json(new { success = false, message = "Thiếu thông tin phim" });
            }

            var comments = await _commentService.GetCommentsByMovieAsync(movieSlug);
            return Json(new { success = true, comments = comments });
        }
    }
}
