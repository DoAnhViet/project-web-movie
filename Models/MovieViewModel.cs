using System.ComponentModel.DataAnnotations;

namespace WebMovie.Models
{
    public class MovieViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Thể loại là bắt buộc")]
        [Display(Name = "Thể loại")]
        public string Genre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quốc gia là bắt buộc")]
        [Display(Name = "Quốc gia")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm sản xuất là bắt buộc")]
        [Range(1900, 2030, ErrorMessage = "Năm phải từ 1900 đến 2030")]
        [Display(Name = "Năm sản xuất")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL poster là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "URL Poster")]
        public string PosterUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL trailer là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "URL Trailer")]
        public string TrailerUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL video là bắt buộc")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        [Display(Name = "URL Video")]
        public string VideoUrl { get; set; } = string.Empty;
    }
}
