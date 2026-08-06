using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ProjectKZM.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjectKZM.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _context;

        public AccountController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.VaiTro)
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.MatKhau == password);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro?.TenVaiTro ?? "User")

            };

            var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Home");

            ViewBag.Error = "Sai thông tin đăng nhập!";
            return View();

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }
    }
}
