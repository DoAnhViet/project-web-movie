using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebMovie.Models;
using WebMovie.Data;
using WebMovie.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Sử dụng In-Memory database tạm thời
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("WebMovieDb"));

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

builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed data
await DataSeeder.SeedAsync(app.Services);

app.Run();
