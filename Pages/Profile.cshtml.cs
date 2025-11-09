using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebMovie.Models;

namespace WebMovie.Pages
{
    /// <summary>
    /// Razor Page Model cho Profile - YÊU CẦU ĐỒ ÁN: Razor Pages
    /// PageModel chứa logic xử lý cho Razor Page
    /// </summary>
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationUser? User { get; set; }
        public IList<string> UserRoles { get; set; } = new List<string>();

        /// <summary>
        /// OnGetAsync - được gọi khi truy cập trang (GET request)
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            User = currentUser;
            UserRoles = await _userManager.GetRolesAsync(currentUser);

            return Page();
        }

        /// <summary>
        /// OnPostLogoutAsync - xử lý đăng xuất (POST request)
        /// </summary>
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Index");
        }
    }
}
