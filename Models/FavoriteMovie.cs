using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMovie.Models
{
    /// <summary>
    /// Lưu phim yêu thích của user
    /// Không cần lưu toàn bộ thông tin phim vì lấy từ API
    /// Chỉ lưu slug để query API khi cần
    /// </summary>
    public class FavoriteMovie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Slug phim từ API (ví dụ: "ban-tay-diet-quy")
        /// </summary>
        [Required]
        [StringLength(200)]
        public string MovieSlug { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề phim (cache từ API để hiển thị nhanh)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string MovieTitle { get; set; } = string.Empty;

        /// <summary>
        /// Tên gốc phim (tiếng Anh/Trung/...)
        /// </summary>
        [StringLength(500)]
        public string OriginName { get; set; } = string.Empty;

        /// <summary>
        /// URL poster (cache từ API)
        /// </summary>
        [StringLength(1000)]
        public string PosterUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL thumbnail (cache từ API)
        /// </summary>
        [StringLength(1000)]
        public string ThumbUrl { get; set; } = string.Empty;

        /// <summary>
        /// Năm phát hành
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Thời gian thêm vào danh sách yêu thích
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
