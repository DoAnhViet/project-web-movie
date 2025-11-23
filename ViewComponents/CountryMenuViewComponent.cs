
using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;
using WebMovie.Models;

namespace WebMovie.ViewComponents
{
    public class CountryMenuViewComponent : ViewComponent
    {
        private readonly MovieApiService _movieApiService;

        public CountryMenuViewComponent(MovieApiService movieApiService)
        {
            _movieApiService = movieApiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var countries = await _movieApiService.GetCountriesAsync();

            var popularSlugs = new List<string>
            {
                "trung-quoc", "han-quoc", "nhat-ban", "thai-lan", "my", "au-my",
                "hong-kong", "an-do", "phap", "anh", "dai-loan", "philippines"
            };

            var filtered = countries?
                .Where(c => c.Slug != null && popularSlugs.Contains(c.Slug.ToLowerInvariant()))
                .OrderBy(c => popularSlugs.IndexOf(c.Slug.ToLowerInvariant()))
                .ToList() ?? new List<Category>();

            return View(filtered);
        }
    }
}