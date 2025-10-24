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
        public async Task<MovieListResponse?> SearchMoviesAsync(string keyword, int page = 1)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/v1/api/tim-kiem?keyword={keyword}&page={page}");
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
                Console.WriteLine($"Error searching movies: {ex.Message}");
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
