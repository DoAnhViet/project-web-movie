using System.Text.Json.Serialization;

namespace WebMovie.Models
{
    public class ApiMovie
    {
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("origin_name")]
        public string OriginName { get; set; } = string.Empty;

        [JsonPropertyName("thumb_url")]
        public string ThumbUrl { get; set; } = string.Empty;

        [JsonPropertyName("poster_url")]
        public string PosterUrl { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("episode_total")]
        public string EpisodeTotal { get; set; } = string.Empty;

        [JsonPropertyName("quality")]
        public string Quality { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [JsonPropertyName("notify")]
        public string Notify { get; set; } = string.Empty;

        [JsonPropertyName("showtimes")]
        public string Showtimes { get; set; } = string.Empty;

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("view")]
        public int View { get; set; }

        [JsonPropertyName("actor")]
        public List<string> Actor { get; set; } = new List<string>();

        [JsonPropertyName("director")]
        public List<string> Director { get; set; } = new List<string>();

        [JsonPropertyName("category")]
        public List<ApiCategory> Category { get; set; } = new List<ApiCategory>();

        [JsonPropertyName("country")]
        public List<ApiCountry> Country { get; set; } = new List<ApiCountry>();
    }

    public class ApiCategory
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
    }

    public class ApiCountry
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        [JsonPropertyName("status")]
        public bool Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public ApiResponseData Data { get; set; } = new ApiResponseData();
    }

    public class ApiResponseData
    {
        [JsonPropertyName("items")]
        public List<ApiMovie> Items { get; set; } = new List<ApiMovie>();

        [JsonPropertyName("params")]
        public ApiParams Params { get; set; } = new ApiParams();

        [JsonPropertyName("pagination")]
        public ApiPagination Pagination { get; set; } = new ApiPagination();
    }

    public class ApiParams
    {
        [JsonPropertyName("pagination")]
        public ApiPaginationInfo Pagination { get; set; } = new ApiPaginationInfo();
    }

    public class ApiPaginationInfo
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

    public class ApiPagination
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
}
