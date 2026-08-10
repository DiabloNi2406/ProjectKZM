using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _context;

        public AccountController(MyDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // LOGIN - HIỂN THỊ TRANG ĐĂNG NHẬP
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        // =====================================================
        // LOGIN - XỬ LÝ ĐĂNG NHẬP
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            var user = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.MatKhau == password);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                return View();
            }


            // =================================================
            // TẠO CLAIM
            // =================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    user.HoTen ?? "Người dùng"
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? ""
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.VaiTro?.TenVaiTro ?? "User"
                )
            };


            // =================================================
            // TẠO IDENTITY
            // =================================================

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );


            var principal = new ClaimsPrincipal(identity);


            // =================================================
            // LƯU COOKIE ĐĂNG NHẬP
            // =================================================

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc =
                        DateTimeOffset.UtcNow.AddDays(7)
                }
            );


            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =====================================================
        // REGISTER - HIỂN THỊ TRANG ĐĂNG KÝ
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        // =====================================================
        // REGISTER - XỬ LÝ ĐĂNG KÝ
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            NguoiDung model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =================================================
            // KIỂM TRA EMAIL ĐÃ TỒN TẠI
            // =================================================

            bool existed = await _context.NguoiDungs
                .AnyAsync(x =>
                    x.Email == model.Email);

            if (existed)
            {
                ViewBag.Error =
                    "Email này đã tồn tại!";

                return View(model);
            }


            // =================================================
            // TỰ ĐỘNG GÁN ROLE USER
            // =================================================

            var userRole = await _context.VaiTros
                .FirstOrDefaultAsync(x =>
                    x.TenVaiTro == "User");

            if (userRole == null)
            {
                ViewBag.Error =
                    "Chưa có vai trò User trong database!";

                return View(model);
            }


            model.MaVaiTro =
                userRole.MaVaiTro;


            // =================================================
            // THÊM NGƯỜI DÙNG
            // =================================================

            _context.NguoiDungs.Add(model);

            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Login)
            );
        }


        // =====================================================
        // PROFILE - XEM THÔNG TIN CÁ NHÂN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // -------------------------------------------------
            // Kiểm tra đăng nhập
            // -------------------------------------------------

            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Lấy email từ Cookie Claim
            // -------------------------------------------------

            var email =
                User.FindFirstValue(ClaimTypes.Email);


            if (string.IsNullOrWhiteSpace(email))
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Tìm người dùng trong Database
            // -------------------------------------------------

            var user = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.Email == email);


            if (user == null)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Hiển thị Profile
            // -------------------------------------------------

            return View(user);
        }


        // =====================================================
        // EDIT PROFILE - HIỂN THỊ TRANG CHỈNH SỬA
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // -------------------------------------------------
            // Kiểm tra đăng nhập
            // -------------------------------------------------

            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Lấy email của người đang đăng nhập
            // -------------------------------------------------

            var email =
                User.FindFirstValue(ClaimTypes.Email);


            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Tìm người dùng
            // -------------------------------------------------

            var user = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.Email == email);


            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Truyền sang View
            // -------------------------------------------------

            return View(user);
        }


        // =====================================================
        // EDIT PROFILE - LƯU THÔNG TIN
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            int id,
            string hoTen,
            string? email,
            string? soDienThoai,
            string? diaChi)
        {
            // -------------------------------------------------
            // Kiểm tra đăng nhập
            // -------------------------------------------------

            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Lấy email hiện tại từ Cookie
            // -------------------------------------------------

            var currentEmail =
                User.FindFirstValue(ClaimTypes.Email);


            if (string.IsNullOrWhiteSpace(currentEmail))
            {
                return RedirectToAction(nameof(Login));
            }


            // -------------------------------------------------
            // Tìm đúng người đang đăng nhập
            // -------------------------------------------------

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x =>
                    x.MaNguoiDung == id &&
                    x.Email == currentEmail);


            if (user == null)
            {
                return RedirectToAction(nameof(Profile));
            }


            // -------------------------------------------------
            // Kiểm tra email mới
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(email) &&
                !email.Equals(
                    currentEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                bool emailExists =
                    await _context.NguoiDungs.AnyAsync(x =>
                        x.Email == email &&
                        x.MaNguoiDung != id);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Email này đã được sử dụng bởi tài khoản khác."
                    );

                    user.HoTen = hoTen;
                    user.Email = email;

                    // Nếu model của bạn có các property này
                    // thì giữ nguyên dữ liệu hiện tại.

                    return View(user);
                }
            }


            // -------------------------------------------------
            // Cập nhật thông tin
            // -------------------------------------------------

            user.HoTen = hoTen;


            if (!string.IsNullOrWhiteSpace(email))
            {
                user.Email = email;
            }


            // -------------------------------------------------
            // Các trường dưới đây chỉ cập nhật nếu model
            // NguoiDung của bạn có property tương ứng.
            // -------------------------------------------------

            user.SoDienThoai = soDienThoai;
            user.DiaChi = diaChi;


            // -------------------------------------------------
            // LƯU DATABASE
            // -------------------------------------------------

            await _context.SaveChangesAsync();


            // -------------------------------------------------
            // Cập nhật lại Cookie Claim
            // -------------------------------------------------

            var role = await _context.VaiTros
                .FirstOrDefaultAsync(x =>
                    x.MaVaiTro == user.MaVaiTro);


            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    user.HoTen ?? "Người dùng"
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? ""
                ),

                new Claim(
                    ClaimTypes.Role,
                    role?.TenVaiTro ?? "User"
                )
            };


            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimTypes.Role
            );


            var principal =
                new ClaimsPrincipal(identity);


            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc =
                        DateTimeOffset.UtcNow.AddDays(7)
                }
            );


            // -------------------------------------------------
            // Thông báo
            // -------------------------------------------------

            TempData["Success"] =
                "Cập nhật thông tin cá nhân thành công!";


            return RedirectToAction(
                nameof(Profile)
            );
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                nameof(Login)
            );
        }


        // =====================================================
        // ACCESS DENIED
        // =====================================================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}