using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMovie.Models
{
    /// <summary>
    /// Bình luận của user về phim
    /// </summary>
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Slug phim từ API
        /// </summary>
        [Required]
        [StringLength(200)]
        public string MovieSlug { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung bình luận
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Đánh giá sao (1-5)
        /// </summary>
        [Range(1, 5)]
        public int? Rating { get; set; }

        /// <summary>
        /// Bình luận cha (nếu là reply)
        /// </summary>
        public int? ParentCommentId { get; set; }

        [ForeignKey("ParentCommentId")]
        public Comment? ParentComment { get; set; }

        /// <summary>
        /// Các bình luận trả lời
        /// </summary>
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        /// <summary>
        /// Số lượng like
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// Đã bị ẩn bởi admin chưa (spam, vi phạm,...)
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
