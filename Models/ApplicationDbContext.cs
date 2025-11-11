using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebMovie.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options){}

        // Tiêu đề phim tùy chỉnh (Việt hóa)
        public DbSet<CustomMovieTitle> CustomMovieTitles { get; set; } = null!;

        // Lịch sử xem phim
        public DbSet<WatchHistory> WatchHistories { get; set; } = null!;

        // Bình luận
        public DbSet<Comment> Comments { get; set; } = null!;

        // Báo cáo từ user
        public DbSet<Report> Reports { get; set; } = null!;

        // Phim yêu thích của user
        public DbSet<FavoriteMovie> FavoriteMovies { get; set; } = null!;

        // Comment phim
        public DbSet<MovieComment> MovieComments { get; set; } = null!;
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình indexes để tối ưu query
            builder.Entity<FavoriteMovie>()
                .HasIndex(f => new { f.UserId, f.MovieSlug })
                .IsUnique(); // User chỉ có thể favorite 1 phim 1 lần

            builder.Entity<CustomMovieTitle>()
                .HasIndex(c => c.MovieSlug)
                .IsUnique(); // Mỗi phim chỉ có 1 tiêu đề custom

            builder.Entity<WatchHistory>()
                .HasIndex(w => new { w.UserId, w.MovieSlug });

            builder.Entity<Comment>()
                .HasIndex(c => new { c.MovieSlug, c.CreatedAt });

            builder.Entity<Comment>()
                .HasIndex(c => c.ParentCommentId);

            builder.Entity<Report>()
                .HasIndex(r => new { r.Status, r.CreatedAt });
        }
    }
}
