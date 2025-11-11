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

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // POST: Thêm comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string movieSlug, string movieTitle, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung comment không được để trống" });
            }

            if (content.Length > 1000)
            {
                return Json(new { success = false, message = "Comment không được quá 1000 ký tự" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var success = await _commentService.AddCommentAsync(userId, movieSlug, movieTitle, content);

            if (success)
            {
                return Json(new { success = true, message = "Đã thêm comment thành công!" });
            }

            return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại" });
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
    }
}
