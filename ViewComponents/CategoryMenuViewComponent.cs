using Microsoft.AspNetCore.Mvc;
using WebMovie.Services;
using WebMovie.Models;

namespace WebMovie.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly MovieApiService _movieApiService;

        public CategoryMenuViewComponent(MovieApiService movieApiService)
        {
            _movieApiService = movieApiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var genres = await _movieApiService.GetGenresAsync();
            // Show a reasonable number in the header (e.g. first 20)
            var model = genres?.Take(20).ToList() ?? new List<Category>();
            return View(model);
        }
    }
}
