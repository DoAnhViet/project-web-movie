using System.ComponentModel.DataAnnotations;

namespace WebMovie.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string Genre { get; set; } = string.Empty;

        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public int Year { get; set; }

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        public string PosterUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string TrailerUrl { get; set; } = string.Empty;

        [StringLength(500)]
        public string VideoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
