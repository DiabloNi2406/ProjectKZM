using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();

// ======================================================
// SQL Server
// ======================================================

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ======================================================
// Authentication
// ======================================================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // Cookie tồn tại 7 ngày
        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        // Gia hạn nếu còn hoạt động
        options.SlidingExpiration = true;
    });

// ======================================================
// Authorization
// ======================================================

builder.Services.AddAuthorization();

// ======================================================
// Session (Chuẩn bị cho Giỏ hàng)
// ======================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ======================================================
// Pipeline
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Session phải đứng trước Authentication
app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();