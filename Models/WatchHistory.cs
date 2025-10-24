using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMovie.Models
{
    /// <summary>
    /// Lịch sử xem phim của user
    /// Lưu lại tập đã xem, thời gian xem đến đâu
    /// </summary>
    public class WatchHistory
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
        /// Tiêu đề phim (cache)
        /// </summary>
        [StringLength(500)]
        public string MovieTitle { get; set; } = string.Empty;

        /// <summary>
        /// Poster URL (cache)
        /// </summary>
        [StringLength(1000)]
        public string PosterUrl { get; set; } = string.Empty;

        /// <summary>
        /// Tên tập đang xem (ví dụ: "Tập 1", "Full")
        /// </summary>
        [StringLength(100)]
        public string EpisodeName { get; set; } = string.Empty;

        /// <summary>
        /// Slug của tập phim (để continue xem)
        /// </summary>
        [StringLength(200)]
        public string EpisodeSlug { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian đã xem (giây)
        /// </summary>
        public int CurrentTime { get; set; }

        /// <summary>
        /// Tổng thời gian tập phim (giây)
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        /// Phần trăm đã xem
        /// </summary>
        public int ProgressPercent => TotalTime > 0 ? (int)((double)CurrentTime / TotalTime * 100) : 0;

        /// <summary>
        /// Đã xem xong tập này chưa
        /// </summary>
        public bool IsCompleted => ProgressPercent >= 90;

        /// <summary>
        /// Lần cuối xem
        /// </summary>
        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lần đầu xem phim này
        /// </summary>
        public DateTime FirstWatchedAt { get; set; } = DateTime.UtcNow;
    }
}
