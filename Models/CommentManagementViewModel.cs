namespace WebMovie.Models
{
    /// <summary>
    /// View Model cho trang quản lý bình luận
    /// </summary>
    public class CommentManagementViewModel
    {
        public List<MovieComment> Comments { get; set; } = new();
        
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalComments { get; set; }
        public int PageSize { get; set; } = 20;
        
        public string? FilterMovie { get; set; }
        public string? SearchQuery { get; set; }

        // Thống kê
        public int CommentsToday { get; set; }
        public int CommentsThisMonth { get; set; }
        public List<MovieCommentStats>? TopMoviesWithComments { get; set; }
        public List<CommentCountByUser>? TopCommenters { get; set; }
    }

    /// <summary>
    /// Thống kê bình luận theo phim
    /// </summary>
    public class MovieCommentStats
    {
        public string MovieSlug { get; set; } = string.Empty;
        public string MovieTitle { get; set; } = string.Empty;
        public int CommentCount { get; set; }
    }

    /// <summary>
    /// Đếm bình luận của người dùng
    /// </summary>
    public class CommentCountByUser
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int CommentCount { get; set; }
    }
}
