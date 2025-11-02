using System.Text.Json;
using WebMovie.Models;

namespace WebMovie.Services
{
    public interface IApiService
    {
        Task<List<ApiMovie>> GetMoviesAsync(int page = 1, int limit = 20);
        Task<ApiMovie?> GetMovieDetailAsync(string slug);
        Task<List<ApiMovie>> SearchMoviesAsync(string keyword, int page = 1, int limit = 20);
        Task<List<ApiCategory>> GetCategoriesAsync();
        Task<List<ApiCountry>> GetCountriesAsync();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly string _baseUrl = "https://phimapi.com";

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

       public async Task<List<ApiMovie>> GetMoviesAsync(int page = 1, int limit = 20)
{
    try
    {
        // Dòng cũ: var url = $"{_baseUrl}/danh-sach?page={page}&limit={limit}";

        // DÒNG SỬA: Đã thêm endpoint chính xác (thường là /danh-sach/phim)
        var url = $"{_baseUrl}/danh-sach/phim?page={page}&limit={limit}"; 
        
        var response = await _httpClient.GetStringAsync(url);
        var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);
        
        return apiResponse?.Data?.Items ?? new List<ApiMovie>();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting movies from API");
        return new List<ApiMovie>();
    }
}

        public async Task<ApiMovie?> GetMovieDetailAsync(string slug)
        {
            try
            {
                var url = $"{_baseUrl}/phim/{slug}";
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                
                return apiResponse?.Data?.Items?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie detail from API");
                return null;
            }
        }

        public async Task<List<ApiMovie>> SearchMoviesAsync(string keyword, int page = 1, int limit = 20)
        {
            try
            {
                var url = $"{_baseUrl}/tim-kiem?keyword={Uri.EscapeDataString(keyword)}&page={page}&limit={limit}";
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                
                return apiResponse?.Data?.Items ?? new List<ApiMovie>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies from API");
                return new List<ApiMovie>();
            }
        }

        public async Task<List<ApiCategory>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{_baseUrl}/the-loai";
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                
                return apiResponse?.Data?.Items?.SelectMany(m => m.Category).Distinct().ToList() ?? new List<ApiCategory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories from API");
                return new List<ApiCategory>();
            }
        }

        public async Task<List<ApiCountry>> GetCountriesAsync()
        {
            try
            {
                var url = $"{_baseUrl}/quoc-gia";
                var response = await _httpClient.GetStringAsync(url);
                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);
                
                return apiResponse?.Data?.Items?.SelectMany(m => m.Country).Distinct().ToList() ?? new List<ApiCountry>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting countries from API");
                return new List<ApiCountry>();
            }
        }
    }
}
