using System.Text.Json;
using WebMovie.Models;
using WebMovie.Data;

namespace WebMovie.Services
{
    public class MovieApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationDbContext? _db;
        private const string BaseUrl = "https://phimapi.com";

        public MovieApiService(HttpClient httpClient, ApplicationDbContext? db = null)
        {
            _httpClient = httpClient;
            _db = db;
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
                
                ApplyCustomTitles(result);
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
                
                ApplyCustomTitles(result);
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

        // Lấy phim theo thể loại với filter chi tiết
        public async Task<MovieListResponse?> GetCategoryDetailAsync(
            string categorySlug, 
            int page = 1,
            string sort_field = "_id",
            string sort_type = "asc",
            string sort_lang = "",
            string country = "",
            string year = "",
            int limit = 20)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"sort_field={sort_field}",
                    $"sort_type={sort_type}",
                    $"limit={limit}"
                };

                if (!string.IsNullOrEmpty(sort_lang)) queryParams.Add($"sort_lang={sort_lang}");
                if (!string.IsNullOrEmpty(country)) queryParams.Add($"country={country}");
                if (!string.IsNullOrEmpty(year)) queryParams.Add($"year={year}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/v1/api/the-loai/{categorySlug}?{query}");
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

        // Lấy phim theo năm với filter chi tiết
        public async Task<MovieListResponse?> GetMoviesByYearAsync(
            string year, 
            int page = 1,
            string sort_field = "_id",
            string sort_type = "asc",
            string sort_lang = "",
            string category = "",
            string country = "",
            int limit = 20)
        {
            try
            {
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"sort_field={sort_field}",
                    $"sort_type={sort_type}",
                    $"limit={limit}"
                };

                if (!string.IsNullOrEmpty(sort_lang)) queryParams.Add($"sort_lang={sort_lang}");
                if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={category}");
                if (!string.IsNullOrEmpty(country)) queryParams.Add($"country={country}");

                var query = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/v1/api/nam/{year}?{query}");
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
                Console.WriteLine($"Error fetching movies by year: {ex.Message}");
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

        // Lấy danh sách quốc gia
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

        // Lấy danh sách thể loại
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
                    Id = g.Slug
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

        #region Custom Titles Support
        // Apply custom titles from database to movie list
        private void ApplyCustomTitles(MovieListResponse? list)
        {
            if (list == null || list.Items == null || _db == null) return;

            var slugs = list.Items.Select(i => i.Slug).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
            if (!slugs.Any()) return;

            var overrides = _db.CustomMovieTitles
                .Where(c => slugs.Contains(c.MovieSlug))
                .ToDictionary(c => c.MovieSlug, c => c);

            foreach (var item in list.Items)
            {
                if (overrides.TryGetValue(item.Slug, out var custom))
                {
                    if (string.IsNullOrEmpty(custom.OriginalTitle))
                    {
                        custom.OriginalTitle = item.Name ?? "";
                    }
                    item.Name = custom.CustomTitle ?? item.Name;
                }
            }
        }

        private void ApplyCustomTitles(MovieDetailResponse? detail)
        {
            if (detail == null || detail.Movie == null || _db == null) return;

            var slug = detail.Movie.Slug;
            if (string.IsNullOrEmpty(slug)) return;

            var custom = _db.CustomMovieTitles.FirstOrDefault(c => c.MovieSlug == slug);
            if (custom != null)
            {
                if (string.IsNullOrEmpty(custom.OriginalTitle))
                {
                    custom.OriginalTitle = detail.Movie.Name ?? "";
                }
                detail.Movie.Name = custom.CustomTitle ?? detail.Movie.Name;
                if (!string.IsNullOrEmpty(custom.CustomDescription))
                {
                    detail.Movie.Content = custom.CustomDescription;
                }
            }
        }
        #endregion
    }
}

// Helper model for API deserialization
public class GenreResponse
{
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
}
