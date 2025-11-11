using Microsoft.AspNetCore.Mvc;

namespace WebMovie.ViewComponents
{
    /// <summary>
    /// SearchBar View Component - YÊU CẦU ĐỒ ÁN: View Components
    /// Thay thế Partial View _SearchBar.cshtml bằng View Component có logic
    /// </summary>
    public class SearchBarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string? currentKeyword = null)
        {
            // Lấy keyword từ query string nếu không truyền vào
            if (string.IsNullOrEmpty(currentKeyword))
            {
                currentKeyword = HttpContext.Request.Query["keyword"].ToString();
            }

            ViewBag.CurrentKeyword = currentKeyword;
            return View("Default", currentKeyword ?? "");
        }
    }
}
