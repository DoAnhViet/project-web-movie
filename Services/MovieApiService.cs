using System.Text.Json;
using WebMovie.Models;

namespace WebMovie.Services
{
    public class MovieApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://phimapi.com";

        public MovieApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        // Lấy danh sách phim mới cập nhật
        public async Task<MovieListResponse?> GetNewMoviesAsync(int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/danh-sach/phim-moi-cap-nhat?page={page}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching movies: {ex.Message}");
                return null;
            }
        }

        // Lấy thông tin chi tiết phim theo slug
        public async Task<MovieDetailResponse?> GetMovieDetailAsync(string slug)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/phim/{slug}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MovieDetailResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching movie detail: {ex.Message}");
                return null;
            }
        }

        // Tìm kiếm phim
        // Trong SearchMoviesAsync - GIỮ /v1/api/tim-kiem (ĐÚNG!)
        public async Task<MovieListResponse?> SearchMoviesAsync(string keyword, int page = 1)
        {
            try
            {
                var encodedKeyword = Uri.EscapeDataString(keyword.Trim());
                var response = await _httpClient.GetAsync($"/v1/api/tim-kiem?keyword={encodedKeyword}&page={page}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<SearchResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (searchResult == null || searchResult.Status?.ToLower() != "success")
                    return null;

                var items = searchResult.Data?.Items ?? searchResult.Items ?? new List<MovieItem>();

                // SỬA: THÊM PREFIX CHO ẢNH TÌM KIẾM
                const string cdn = "https://phimimg.com";
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.PosterUrl) && !item.PosterUrl.StartsWith("http"))
                        item.PosterUrl = cdn + "/" + item.PosterUrl.TrimStart('/');
                    if (!string.IsNullOrEmpty(item.ThumbUrl) && !item.ThumbUrl.StartsWith("http"))
                        item.ThumbUrl = cdn + "/" + item.ThumbUrl.TrimStart('/');
                }

                return new MovieListResponse
                {
                    Status = true,
                    Message = searchResult.Message,
                    Items = items,
                    Pagination = searchResult.Data?.Pagination ?? searchResult.Pagination
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching: {ex.Message}");
                return null;
            }
        }

        // Lấy phim theo thể loại
        public async Task<MovieListResponse?> GetMoviesByCategoryAsync(string categorySlug, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/api/the-loai/{categorySlug}?page={page}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching category movies: {ex.Message}");
                return null;
            }
        }

        // Lấy phim theo quốc gia
        public async Task<MovieListResponse?> GetMoviesByCountryAsync(string countrySlug, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/api/quoc-gia/{countrySlug}?page={page}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching country movies: {ex.Message}");
                return null;
            }
        }
    }
}
