using System.ComponentModel.DataAnnotations;

namespace WebMovie.Models
{
    public class MovieComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string MovieSlug { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string MovieTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ApplicationUser? User { get; set; }
    }
}
