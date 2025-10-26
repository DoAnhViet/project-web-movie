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
                var url = $"/v1/api/the-loai/{categorySlug}?page={page}";
                Console.WriteLine($"[DEBUG] Fetching category movies: {BaseUrl}{url}");

                var response = await _httpClient.GetAsync(url);
                Console.WriteLine($"[DEBUG] Status: {response.StatusCode}");

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[DEBUG] Content: {content}");

                response.EnsureSuccessStatusCode();

                var result = JsonSerializer.Deserialize<MovieListResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error fetching category movies: {ex.Message}");
                return null;
            }
        }



        
        // Lấy chi tiết thể loại phim (có lọc, sắp xếp, phân trang)
        // Lấy chi tiết thể loại phim (có lọc, sắp xếp, phân trang)
        public async Task<MovieListResponse?> GetCategoryDetailAsync(
            string categorySlug,
            int page = 1,
            string sortField = "_id",
            string sortType = "asc",
            string sortLang = "",
            string country = "",
            string year = "",
            int limit = 20)
        {
            try
            {
                var queryParams = new List<string> { $"page={page}" };

                if (!string.IsNullOrWhiteSpace(sortField))
                    queryParams.Add($"sort_field={sortField}");
                if (!string.IsNullOrWhiteSpace(sortType))
                    queryParams.Add($"sort_type={sortType}");
                if (!string.IsNullOrWhiteSpace(sortLang))
                    queryParams.Add($"sort_lang={sortLang}");
                if (!string.IsNullOrWhiteSpace(country))
                    queryParams.Add($"country={country}");
                if (!string.IsNullOrWhiteSpace(year))
                    queryParams.Add($"year={year}");
                if (limit > 0)
                    queryParams.Add($"limit={limit}");

                var url = $"/v1/api/the-loai/{categorySlug}?{string.Join("&", queryParams)}";

                Console.WriteLine($"[DEBUG] Fetching category detail: {url}");

                var response = await _httpClient.GetAsync(url);
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
                Console.WriteLine($"Error fetching category detail: {ex.Message}");
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

        public async Task<List<Category>?> GetCountriesAsync()
        {
            try
            {
                Console.WriteLine("Calling GET /quoc-gia");
                var response = await _httpClient.GetAsync("/quoc-gia");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content}");

                var countries = JsonSerializer.Deserialize<List<GenreResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var result = countries?.Select(c => new Category
                {
                    Name = c.Name,
                    Slug = c.Slug,
                    Id = c.Slug
                }).ToList() ?? new List<Category>();

                Console.WriteLine($"Parsed {result.Count} countries");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching countries: {ex.Message}");
                return new List<Category>();
            }
        }


        public async Task<List<Category>?> GetGenresAsync()
        {
            try
            {
                Console.WriteLine("Calling GET /the-loai");
                var response = await _httpClient.GetAsync("/the-loai");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content}");

                var genres = JsonSerializer.Deserialize<List<GenreResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var categories = genres?.Select(g => new Category
                {
                    Name = g.Name,
                    Slug = g.Slug,
                    Id = g.Slug // Use slug as ID since API doesn't return one
                }).ToList() ?? new List<Category>();

                Console.WriteLine($"Parsed {categories.Count} categories");
                return categories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching genres: {ex.Message}");
                return new List<Category>();
            }
        }
        public async Task<MovieListResponse?> GetMoviesByYearAsync(
            int year,
            int page = 1,
            string sort_field = "_id",
            string sort_type = "asc",
            string sort_lang = "",
            string category = "",
            string country = "",
            int limit = 20)
        {
            var url = $"/v1/api/nam/{year}?page={page}&sort_field={sort_field}&sort_type={sort_type}&sort_lang={sort_lang}&category={category}&country={country}&limit={limit}";
            return await GetFromApiAsync<MovieListResponse>(url);
        }

        private async Task<T?> GetFromApiAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API ERROR] {url} -> {ex.Message}");
                return default;
            }
        }
    }
}
