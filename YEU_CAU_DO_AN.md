# Báo Cáo Đồ Án - WebMovie

## Yêu Cầu Và Thực Hiện

### ✅ 1. Bootstrap
**Yêu cầu**: Sử dụng Bootstrap framework cho giao diện

**Thực hiện**: 
- Bootstrap 5 đã được tích hợp trong toàn bộ project
- File: `wwwroot/lib/bootstrap/`
- Sử dụng trong: 
  - `Views/Shared/_Layout.cshtml` - Layout chính
  - `Views/Shared/_AdminLayout.cshtml` - Layout admin
  - Tất cả các Views sử dụng các class Bootstrap như: `container`, `row`, `col-md-*`, `card`, `btn`, etc.

**Ví dụ**: 
```html
<div class="container">
    <div class="row">
        <div class="col-md-3">
            <div class="card">...</div>
        </div>
    </div>
</div>
```

---

### ✅ 2. View Components
**Yêu cầu**: Sử dụng View Components trong ASP.NET Core

**Thực hiện**: Đã tạo 2 View Components được sử dụng trong các trang thực tế

#### a) SearchBarViewComponent
- **File**: `ViewComponents/SearchBarViewComponent.cs`
- **View**: `Views/Shared/Components/SearchBar/Default.cshtml`
- **Chức năng**: Component tìm kiếm phim với từ khóa
- **Sử dụng tại**: 
  - `Views/Shared/_Layout.cshtml` (line ~50): `@await Component.InvokeAsync("SearchBar")`
  - `Views/Shared/_AdminLayout.cshtml` (line ~90): `@await Component.InvokeAsync("SearchBar")`

**Code**:
```csharp
public class SearchBarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var keyword = Request.Query["keyword"].ToString();
        return View("Default", keyword);
    }
}
```

#### b) MovieCardViewComponent
- **File**: `ViewComponents/MovieCardViewComponent.cs`
- **View**: `Views/Shared/Components/MovieCard/Default.cshtml`
- **Chức năng**: Component hiển thị thẻ phim (poster, tên, thông tin)
- **Sử dụng tại**:
  - `Views/Home/Index.cshtml` (line ~100): `@await Component.InvokeAsync("MovieCard", movie)`
  - `Views/Movie/NewMovies.cshtml` (line ~30): `@await Component.InvokeAsync("MovieCard", movie)`

**Code**:
```csharp
public class MovieCardViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(MovieItem movie)
    {
        return View("Default", movie);
    }
}
```

---

### ✅ 3. Razor Pages
**Yêu cầu**: Sử dụng Razor Pages (không phải MVC Views thông thường)

**Thực hiện**: Đã chuyển đổi trang Profile sang Razor Page

#### Profile Razor Page
- **File**: 
  - `Pages/Profile.cshtml` - View với `@page` directive
  - `Pages/Profile.cshtml.cs` - PageModel với logic
- **Chức năng**: Trang thông tin người dùng, đăng xuất
- **Routing**: `/Profile`

**Đặc điểm Razor Page**:
```csharp
// Profile.cshtml
@page
@model WebMovie.Pages.ProfileModel

// Profile.cshtml.cs
public class ProfileModel : PageModel
{
    public async Task OnGetAsync()
    {
        // Load user data
    }
    
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        // Logout logic
    }
}
```

**Cấu hình trong Program.cs**:
```csharp
builder.Services.AddRazorPages(); // Đăng ký Razor Pages
app.MapRazorPages(); // Map routing cho Razor Pages
```

---

### ✅ 4. Blazor
**Yêu cầu**: Sử dụng Blazor (Server-side hoặc WebAssembly)

**Thực hiện**: Đã tích hợp Blazor Server vào trang Favorites

#### FavoriteMoviesComponent (Blazor)
- **File**: `Components/FavoriteMoviesComponent.razor`
- **Loại**: Blazor Server Component
- **Chức năng**: Hiển thị danh sách phim yêu thích với khả năng xóa real-time (không reload trang)
- **Sử dụng tại**: `Views/Favorite/Index.cshtml`

**Tính năng Blazor**:
- ✅ `@code` block với C# logic
- ✅ Async operations: `LoadFavorites()`, `RemoveFromFavorites()`
- ✅ State management: `StateHasChanged()`
- ✅ Real-time UI updates (không reload trang)
- ✅ Event handling: `@onclick="() => RemoveFromFavorites(movie.Slug)"`
- ✅ Conditional rendering: `@if (isLoading)`, `@if (movies.Any())`

**Code mẫu**:
```razor
@inject FavoriteService FavoriteService
@inject UserManager<ApplicationUser> UserManager

<div class="container mt-4">
    @if (isLoading)
    {
        <div class="text-center">
            <div class="spinner-border" role="status"></div>
        </div>
    }
    else if (movies.Any())
    {
        @foreach (var movie in movies)
        {
            <button @onclick="() => RemoveFromFavorites(movie.Slug)" 
                    class="btn btn-sm btn-danger">
                <i class="fas fa-trash"></i>
            </button>
        }
    }
</div>

@code {
    private List<MovieItem> movies = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadFavorites();
    }

    private async Task RemoveFromFavorites(string slug)
    {
        // Real-time remove without page reload
        await FavoriteService.RemoveFavoriteAsync(userId, slug);
        await LoadFavorites();
        StateHasChanged();
    }
}
```

**Cấu hình trong Program.cs**:
```csharp
builder.Services.AddServerSideBlazor(); // Đăng ký Blazor Server
app.MapBlazorHub(); // Map SignalR hub cho Blazor
```

**Render trong View**:
```html
<!-- Views/Favorite/Index.cshtml -->
<component type="typeof(WebMovie.Components.FavoriteMoviesComponent)" 
           render-mode="ServerPrerendered" />

<script src="_framework/blazor.server.js"></script>
```

---

## Tổng Kết

| Yêu Cầu | Trạng Thái | File/Vị Trí | Chức Năng |
|---------|------------|-------------|-----------|
| **Bootstrap** | ✅ Hoàn thành | `wwwroot/lib/bootstrap/` | Toàn bộ giao diện |
| **View Components** | ✅ Hoàn thành | `ViewComponents/SearchBarViewComponent.cs`<br>`ViewComponents/MovieCardViewComponent.cs` | Tìm kiếm phim<br>Hiển thị thẻ phim |
| **Razor Pages** | ✅ Hoàn thành | `Pages/Profile.cshtml`<br>`Pages/Profile.cshtml.cs` | Trang thông tin người dùng |
| **Blazor** | ✅ Hoàn thành | `Components/FavoriteMoviesComponent.razor` | Danh sách yêu thích tương tác real-time |

### Điểm Đặc Biệt:
- ✅ Tất cả các yêu cầu đều được **tích hợp vào các tính năng thực tế** của website, không phải demo
- ✅ View Components được sử dụng nhiều nơi (SearchBar ở header, MovieCard ở danh sách phim)
- ✅ Razor Pages có logic thực tế (đăng xuất, hiển thị thông tin user)
- ✅ Blazor Component có tương tác thực sự (xóa phim yêu thích không cần reload trang)

### Kiểm Tra:
```bash
# Build project
dotnet build

# Run project
dotnet run

# Truy cập:
# - View Component: Vào trang chủ, xem thanh tìm kiếm và danh sách phim
# - Razor Page: Truy cập /Profile
# - Blazor: Đăng nhập, vào /Favorite, thử xóa phim yêu thích (không reload trang)
```
