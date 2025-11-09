using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using WebMovie.Data;
using WebMovie.Services;
using DotNetEnv;

// Load .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Sử dụng MySQL Cloud từ Aiven
// Try to read connection string from environment (.env) first, then appsettings
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                       ?? builder.Configuration.GetConnectionString("DefaultConnection");

var useInMemory = false;
if (string.IsNullOrEmpty(connectionString))
{
    // If no connection string is provided, fall back to an in-memory database for development so app can start.
    // This prevents startup from throwing when DB isn't configured.
    useInMemory = true;
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("WebMovie_Dev_InMemory"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            )
        ));
}

// Đăng ký FavoriteService
builder.Services.AddScoped<FavoriteService>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình password
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    
    // Cấu hình user
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// YÊU CẦU ĐỒ ÁN: Thêm Razor Pages support
builder.Services.AddRazorPages();

// YÊU CẦU ĐỒ ÁN: Thêm Blazor Server support
builder.Services.AddServerSideBlazor();

builder.Services.AddControllersWithViews();



// ======================= GOOGLE AUTH =======================
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      //  options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    })
    .AddGoogle(options =>
    {
        // Lấy từ appsettings.json 
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
                           ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");

        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
                               ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

        options.CallbackPath = "/signin-google"; // Google mặc định gọi lại đường dẫn này
    });



// Đăng ký HttpClient cho MovieApiService
builder.Services.AddHttpClient<MovieApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// YÊU CẦU ĐỒ ÁN: Map Razor Pages
app.MapRazorPages();

// YÊU CẦU ĐỒ ÁN: Map Blazor Hub
app.MapBlazorHub();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data (only if using a real database)
if (!useInMemory)
{
    try
    {
        await DataSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        // Log and continue; seeding failures shouldn't prevent the app from starting in development.
        app.Logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();


