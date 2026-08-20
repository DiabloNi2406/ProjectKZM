using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.Services;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// MVC
// ======================================================

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<EmailService>();

// ======================================================
// SQL SERVER
// ======================================================

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ======================================================
// VNPAY
// ======================================================

builder.Services.Configure<VNPaySettings>(
    builder.Configuration.GetSection("VNPay"));


// ======================================================
// AUTHENTICATION
// ======================================================

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        // Cookie tồn tại 7 ngày
        options.ExpireTimeSpan =
            TimeSpan.FromDays(7);

        // Gia hạn nếu người dùng còn hoạt động
        options.SlidingExpiration = true;
    });


// ======================================================
// AUTHORIZATION
// ======================================================

builder.Services.AddAuthorization();


// ======================================================
// SESSION
// ======================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout =
        TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;

    options.Cookie.IsEssential = true;
});


// ======================================================
// BUILD APP
// ======================================================

var app = builder.Build();


// ======================================================
// PIPELINE
// ======================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ======================================================
// HTTPS
// ======================================================

app.UseHttpsRedirection();


// ======================================================
// STATIC FILES
// ======================================================

app.UseStaticFiles();


// ======================================================
// ROUTING
// ======================================================

app.UseRouting();


// ======================================================
// SESSION
// Phải đặt trước Authentication
// ======================================================

app.UseSession();


// ======================================================
// AUTHENTICATION
// ======================================================

app.UseAuthentication();


// ======================================================
// AUTHORIZATION
// ======================================================

app.UseAuthorization();


// ======================================================
// MVC ROUTE
// ======================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ======================================================
// RUN
// ======================================================

app.Run();