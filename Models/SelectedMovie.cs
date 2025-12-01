using System.ComponentModel.DataAnnotations;

namespace WebMovie.Models
{
    /// <summary>
    /// Phim được admin đánh dấu "đã chọn" (featured/selected)
    /// </summary>
    public class SelectedMovie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string MovieSlug { get; set; } = string.Empty;

        // Dùng để sắp xếp khi hiển thị (giá trị càng nhỏ hiển thị càng trước)
        public int Order { get; set; } = 0;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public string? AddedBy { get; set; }
    }
}
