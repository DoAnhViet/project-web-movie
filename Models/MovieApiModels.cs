using System.Text.Json.Serialization;

namespace WebMovie.Models
{
    // Model cho response danh sách phim
    public class MovieListResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        [JsonPropertyName("items")]
        public List<MovieItem>? Items { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }
    }

    public class PaginationInfo
    {
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonPropertyName("totalItemsPerPage")]
        public int TotalItemsPerPage { get; set; }

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }

    public class MovieItem
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("origin_name")]
        public string OriginName { get; set; }

        [JsonPropertyName("poster_url")]
        public string PosterUrl { get; set; }

        [JsonPropertyName("thumb_url")]
        public string ThumbUrl { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("episode_current")]
        public string EpisodeCurrent { get; set; }

        [JsonPropertyName("category")]
        public List<Category> Category { get; set; }

        [JsonPropertyName("country")]
        public List<Category> Country { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }
    }



    // Model cho thông tin chi tiết phim
    public class MovieDetailResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        [JsonPropertyName("movie")]
        public MovieDetail? Movie { get; set; }

        [JsonPropertyName("episodes")]
        public List<ServerData>? Episodes { get; set; }
    }

    public class MovieDetail
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("origin_name")]
        public string OriginName { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("poster_url")]
        public string PosterUrl { get; set; }

        [JsonPropertyName("thumb_url")]
        public string ThumbUrl { get; set; }

        [JsonPropertyName("trailer_url")]
        public string TrailerUrl { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("episode_current")]
        public string EpisodeCurrent { get; set; }

        [JsonPropertyName("episode_total")]
        public string EpisodeTotal { get; set; }

        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        [JsonPropertyName("lang")]
        public string Lang { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("actor")]
        public List<string> Actor { get; set; }

        [JsonPropertyName("director")]
        public List<string> Director { get; set; }

        [JsonPropertyName("category")]
        public List<Category> Category { get; set; }

        [JsonPropertyName("country")]
        public List<Category> Country { get; set; }
    }

    public class ServerData
    {
        [JsonPropertyName("server_name")]
        public string ServerName { get; set; }

        [JsonPropertyName("server_data")]
        public List<EpisodeData> ServerDataList { get; set; }
    }

    public class EpisodeData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("link_embed")]
        public string LinkEmbed { get; set; }

        [JsonPropertyName("link_m3u8")]
        public string LinkM3u8 { get; set; }
    }

    public class GenreResponse 
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
    }
}
