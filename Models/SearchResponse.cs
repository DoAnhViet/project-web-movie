// Models/SearchResponse.cs
using System.Text.Json.Serialization;

namespace WebMovie.Models
{
    public class SearchResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; } // "success"

        [JsonPropertyName("msg")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public SearchDataWrapper? Data { get; set; }

        // Fallback nếu không có data
        [JsonPropertyName("items")]
        public List<MovieItem>? Items { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }
    }

    public class SearchDataWrapper
    {
        [JsonPropertyName("items")]
        public List<MovieItem>? Items { get; set; }

        [JsonPropertyName("pagination")]
        public PaginationInfo? Pagination { get; set; }

        [JsonPropertyName("seoOnPage")]
        public object? SeoOnPage { get; set; }

        [JsonPropertyName("breadCrumb")]
        public List<object>? BreadCrumb { get; set; }

        [JsonPropertyName("titlePage")]
        public string? TitlePage { get; set; }
    }
}