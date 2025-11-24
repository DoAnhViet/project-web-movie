using Microsoft.AspNetCore.Identity;

namespace WebMovie.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? AvatarUrl { get; set; }

        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
