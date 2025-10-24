using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMovie.Models
{
    /// <summary>
    /// Report/báo cáo từ user về phim hoặc vấn đề khác
    /// </summary>
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Loại report
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ReportType { get; set; } = string.Empty;
        // Các loại: "BrokenLink", "WrongInfo", "SpamComment", "InappropriateContent", "Other"

        /// <summary>
        /// Slug phim (nếu report về phim)
        /// </summary>
        [StringLength(200)]
        public string? MovieSlug { get; set; }

        /// <summary>
        /// Tiêu đề phim (cache)
        /// </summary>
        [StringLength(500)]
        public string? MovieTitle { get; set; }

        /// <summary>
        /// Comment ID (nếu report về comment)
        /// </summary>
        public int? CommentId { get; set; }

        /// <summary>
        /// Tiêu đề report
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái xử lý
        /// </summary>
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        // Các trạng thái: "Pending", "InProgress", "Resolved", "Rejected"

        /// <summary>
        /// Admin note khi xử lý
        /// </summary>
        [StringLength(1000)]
        public string? AdminNote { get; set; }

        /// <summary>
        /// Admin đã xử lý
        /// </summary>
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian xử lý
        /// </summary>
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// Enum cho loại report (để dùng trong dropdown)
    /// </summary>
    public static class ReportTypes
    {
        public const string BrokenLink = "BrokenLink";
        public const string WrongInfo = "WrongInfo";
        public const string SpamComment = "SpamComment";
        public const string InappropriateContent = "InappropriateContent";
        public const string Other = "Other";

        public static Dictionary<string, string> GetDisplayNames() => new()
        {
            { BrokenLink, "Link phim bị hỏng" },
            { WrongInfo, "Thông tin phim sai" },
            { SpamComment, "Bình luận spam" },
            { InappropriateContent, "Nội dung không phù hợp" },
            { Other, "Khác" }
        };
    }
}
