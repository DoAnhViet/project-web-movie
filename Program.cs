using Microsoft.AspNetCore.Identity;
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
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DB_CONNECTION_STRING not found in .env file");
}

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
