using System.ComponentModel.DataAnnotations;

namespace WebMovie.Models
{
    /// <summary>
    /// Cho phép admin thay đổi tiêu đề phim hiển thị (Việt hóa)
    /// Map slug API với tiêu đề tùy chỉnh
    /// </summary>
    public class CustomMovieTitle
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Slug phim từ API (unique)
        /// </summary>
        [Required]
        [StringLength(200)]
        public string MovieSlug { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề tùy chỉnh (tiếng Việt)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string CustomTitle { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề gốc từ API (để tham khảo)
        /// </summary>
        [StringLength(500)]
        public string OriginalTitle { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả tùy chỉnh (nếu cần)
        /// </summary>
        [StringLength(2000)]
        public string? CustomDescription { get; set; }

        /// <summary>
        /// Admin đã cập nhật
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Admin ID đã cập nhật
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
