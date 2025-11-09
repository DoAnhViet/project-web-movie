using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebMovie.Services;

namespace WebMovie.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly MovieApiService _movieApiService;

        protected BaseController(MovieApiService movieApiService)
        {
            _movieApiService = movieApiService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // TODO: Implement GetGenresAsync and GetCountriesAsync in MovieApiService if needed
            // Load thể loại và quốc gia cho mọi view
            // var categories = await _movieApiService.GetGenresAsync();
            // var countries = await _movieApiService.GetCountriesAsync();

            // ViewBag.Categories = categories;
            // ViewBag.Countries = countries;

            await next();
        }
    }
}
