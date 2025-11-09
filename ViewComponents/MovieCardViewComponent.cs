using Microsoft.AspNetCore.Mvc;
using WebMovie.Models;

namespace WebMovie.ViewComponents
{
    /// <summary>
    /// MovieCard View Component - YÊU CẦU ĐỒ ÁN: View Components
    /// Component tái sử dụng để hiển thị thẻ phim
    /// </summary>
    public class MovieCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(MovieItem movie)
        {
            return View(movie);
        }
    }
}
