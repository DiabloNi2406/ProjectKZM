using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.Services;
using ProjectKZM.ViewModels;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProjectKZM.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyDbContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            MyDbContext context,
            EmailService emailService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _emailService = emailService;
            _environment = environment;
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

        // =========================================================
        // QUÊN MẬT KHẨU
        // =========================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }


        // =========================================================
        // GỬI OTP
        // =========================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(
            QuenMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string email =
                model.Email.Trim();


            var user =
                await _context.NguoiDungs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Email == email);


            // Không tiết lộ email có tồn tại hay không.
            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Nếu Email tồn tại trong hệ thống, mã OTP sẽ được gửi đến Email đó.");

                return View(model);
            }


            // =====================================================
            // TẠO OTP 6 SỐ
            // =====================================================

            string otp =
                RandomNumberGenerator
                    .GetInt32(
                        100000,
                        1000000)
                    .ToString();


            // =====================================================
            // HASH OTP TRƯỚC KHI LƯU SESSION
            // =====================================================

            string otpHash =
                HashOtp(otp);


            // =====================================================
            // SESSION
            // =====================================================

            HttpContext.Session.SetString(
                "ForgotPassword_Email",
                email);

            HttpContext.Session.SetString(
                "ForgotPassword_OtpHash",
                otpHash);

            HttpContext.Session.SetString(
                "ForgotPassword_Expires",
                DateTime.UtcNow
                    .AddMinutes(5)
                    .ToString("O"));

            HttpContext.Session.SetInt32(
                "ForgotPassword_Attempts",
                0);


            // =====================================================
            // GỬI EMAIL
            // =====================================================

            try
            {
                await _emailService.SendOtpAsync(
                    email,
                    otp);
            }
            catch
            {
                HttpContext.Session.Remove(
                    "ForgotPassword_Email");

                HttpContext.Session.Remove(
                    "ForgotPassword_OtpHash");

                HttpContext.Session.Remove(
                    "ForgotPassword_Expires");

                HttpContext.Session.Remove(
                    "ForgotPassword_Attempts");


                ModelState.AddModelError(
                    "",
                    "Không thể gửi Email OTP. Vui lòng thử lại sau.");

                return View(model);
            }


            return RedirectToAction(
                nameof(XacNhanOTP));
        }


        // =========================================================
        // NHẬP OTP
        // =========================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult XacNhanOTP()
        {
            string? email =
                HttpContext.Session.GetString(
                    "ForgotPassword_Email");


            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(
                    nameof(QuenMatKhau));
            }


            ViewBag.Email = email;

            return View();
        }


        // =========================================================
        // KIỂM TRA OTP
        // =========================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XacNhanOTP(
            XacNhanOTPViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? email =
                HttpContext.Session.GetString(
                    "ForgotPassword_Email");

            string? savedOtpHash =
                HttpContext.Session.GetString(
                    "ForgotPassword_OtpHash");

            string? expiresString =
                HttpContext.Session.GetString(
                    "ForgotPassword_Expires");

            int attempts =
                HttpContext.Session.GetInt32(
                    "ForgotPassword_Attempts")
                ?? 0;


            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(savedOtpHash) ||
                string.IsNullOrWhiteSpace(expiresString))
            {
                ModelState.AddModelError(
                    "",
                    "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");

                return View(model);
            }


            // =====================================================
            // GIỚI HẠN 5 LẦN
            // =====================================================

            if (attempts >= 5)
            {
                ClearForgotPasswordSession();

                ModelState.AddModelError(
                    "",
                    "Bạn đã nhập sai OTP quá số lần cho phép. Vui lòng yêu cầu mã mới.");

                return View(model);
            }


            // =====================================================
            // KIỂM TRA THỜI GIAN
            // =====================================================

            if (!DateTime.TryParse(
                expiresString,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTime expiresAt))
            {
                ClearForgotPasswordSession();

                ModelState.AddModelError(
                    "",
                    "Mã OTP không hợp lệ. Vui lòng yêu cầu mã mới.");

                return View(model);
            }


            if (DateTime.UtcNow > expiresAt)
            {
                ClearForgotPasswordSession();

                ModelState.AddModelError(
                    "",
                    "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");

                return View(model);
            }


            // =====================================================
            // TĂNG SỐ LẦN THỬ
            // =====================================================

            attempts++;

            HttpContext.Session.SetInt32(
                "ForgotPassword_Attempts",
                attempts);


            // =====================================================
            // SO SÁNH OTP
            // =====================================================

            string enteredOtpHash =
                HashOtp(
                    model.OTP.Trim());


            if (!string.Equals(
                enteredOtpHash,
                savedOtpHash,
                StringComparison.Ordinal))
            {
                ModelState.AddModelError(
                    "",
                    $"Mã OTP không đúng. Bạn còn {5 - attempts} lần thử.");

                return View(model);
            }


            // =====================================================
            // OTP ĐÚNG
            // =====================================================

            HttpContext.Session.SetString(
                "ForgotPassword_Verified",
                "true");


            return RedirectToAction(
                nameof(DatLaiMatKhau));
        }


        // =========================================================
        // ĐẶT LẠI MẬT KHẨU
        // =========================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            string? email =
                HttpContext.Session.GetString(
                    "ForgotPassword_Email");

            string? verified =
                HttpContext.Session.GetString(
                    "ForgotPassword_Verified");


            if (string.IsNullOrWhiteSpace(email) ||
                verified != "true")
            {
                return RedirectToAction(
                    nameof(QuenMatKhau));
            }


            return View();
        }


        // =========================================================
        // LƯU MẬT KHẨU MỚI
        // =========================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(
            DatLaiMatKhauViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? email =
                HttpContext.Session.GetString(
                    "ForgotPassword_Email");

            string? verified =
                HttpContext.Session.GetString(
                    "ForgotPassword_Verified");


            if (string.IsNullOrWhiteSpace(email) ||
                verified != "true")
            {
                return RedirectToAction(
                    nameof(QuenMatKhau));
            }


            var user =
                await _context.NguoiDungs
                    .FirstOrDefaultAsync(
                        x =>
                            x.Email == email);


            if (user == null)
            {
                ClearForgotPasswordSession();

                TempData["Error"] =
                    "Không tìm thấy tài khoản.";

                return RedirectToAction(
                    nameof(Login));
            }


            // =====================================================
            // CẬP NHẬT MẬT KHẨU
            // =====================================================

            user.MatKhau =
                model.MatKhauMoi;


            await _context.SaveChangesAsync();


            // =====================================================
            // XÓA SESSION OTP
            // =====================================================

            ClearForgotPasswordSession();


            TempData["Success"] =
                "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.";


            return RedirectToAction(
                nameof(Login));
        }


        // =========================================================
        // HASH OTP
        // =========================================================

        private static string HashOtp(
            string otp)
        {
            byte[] bytes =
                Encoding.UTF8.GetBytes(otp);

            byte[] hash =
                SHA256.HashData(bytes);

            return Convert
                .ToHexString(hash)
                .ToLowerInvariant();
        }


        // =========================================================
        // XÓA SESSION QUÊN MẬT KHẨU
        // =========================================================

        private void ClearForgotPasswordSession()
        {
            HttpContext.Session.Remove(
                "ForgotPassword_Email");

            HttpContext.Session.Remove(
                "ForgotPassword_OtpHash");

            HttpContext.Session.Remove(
                "ForgotPassword_Expires");

            HttpContext.Session.Remove(
                "ForgotPassword_Attempts");

            HttpContext.Session.Remove(
                "ForgotPassword_Verified");
        }

        // =====================================================
        // XÓA SESSION ĐĂNG KÝ
        // =====================================================

        private void ClearRegisterSession()
        {
            HttpContext.Session.Remove(
                "Register_PendingUser");

            HttpContext.Session.Remove(
                "Register_OtpHash");

            HttpContext.Session.Remove(
                "Register_OtpExpires");

            HttpContext.Session.Remove(
                "Register_OtpAttempts");
        }

        // =====================================================
        // REGISTER - HIỂN THỊ TRANG ĐĂNG KÝ
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // =====================================================
        // REGISTER - XỬ LÝ ĐĂNG KÝ
        // GỬI OTP XÁC MINH EMAIL
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
        NguoiDung model,
        string? confirmPassword,
        IFormFile? anhDaiDienFile)
        {
            // =================================================
            // KIỂM TRA MẬT KHẨU XÁC NHẬN
            // =================================================

            if (string.IsNullOrWhiteSpace(confirmPassword) ||
                model.MatKhau != confirmPassword)
            {
                ModelState.AddModelError(
                    "MatKhau",
                    "Mật khẩu xác nhận không khớp.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // =================================================
            // ẢNH ĐẠI DIỆN
            // =================================================

            if (anhDaiDienFile != null &&
                anhDaiDienFile.Length > 0)
            {
                // Chỉ cho phép một số định dạng ảnh
                var allowedExtensions =
                    new[]
                    {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
                    };

                var extension =
                    Path.GetExtension(
                        anhDaiDienFile.FileName)
                        .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "AnhDaiDien",
                        "Ảnh đại diện không đúng định dạng."
                    );

                    return View(model);
                }


                // Giới hạn 5MB
                const long maxFileSize =
                    5 * 1024 * 1024;

                if (anhDaiDienFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "AnhDaiDien",
                        "Ảnh đại diện không được lớn hơn 5MB."
                    );

                    return View(model);
                }


                // =================================================
                // TẠO THƯ MỤC
                // =================================================

                string uploadFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "avatars"
                    );

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(
                        uploadFolder
                    );
                }


                // =================================================
                // TẠO TÊN FILE MỚI
                // =================================================

                string fileName =
                    Guid.NewGuid().ToString()
                    + extension;


                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName
                    );


                // =================================================
                // LƯU FILE
                // =================================================

                using (
                    var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create
                        )
                )
                {
                    await anhDaiDienFile.CopyToAsync(
                        stream
                    );
                }


                // =================================================
                // LƯU ĐƯỜNG DẪN VÀO MODEL
                // =================================================

                model.AnhDaiDien =
                    "/uploads/avatars/" + fileName;
            }


            // =================================================
            // EMAIL
            // =================================================

            string email = model.Email.Trim();

            model.Email = email;


            // =================================================
            // KIỂM TRA EMAIL ĐÃ TỒN TẠI
            // =================================================

            bool existed =
                await _context.NguoiDungs
                    .AnyAsync(x =>
                        x.Email == email);

            if (existed)
            {
                ViewBag.Error =
                    "Email này đã được đăng ký!";

                return View(model);
            }


            // =================================================
            // ROLE USER
            // =================================================

            var userRole =
                await _context.VaiTros
                    .FirstOrDefaultAsync(
                        x => x.TenVaiTro == "User");

            if (userRole == null)
            {
                ViewBag.Error =
                    "Chưa có vai trò User trong database!";

                return View(model);
            }

            model.MaVaiTro =
                userRole.MaVaiTro;


            // =================================================
            // TẠO OTP 6 SỐ
            // =================================================

            string otp =
                RandomNumberGenerator
                    .GetInt32(
                        100000,
                        1000000)
                    .ToString();

            string otpHash =
                HashOtp(otp);


            // =================================================
            // LƯU THÔNG TIN ĐĂNG KÝ TẠM THỜI
            // =================================================

            string pendingUser =
                System.Text.Json.JsonSerializer.Serialize(
                    model);

            HttpContext.Session.SetString(
                "Register_PendingUser",
                pendingUser);

            HttpContext.Session.SetString(
                "Register_OtpHash",
                otpHash);

            HttpContext.Session.SetString(
                "Register_OtpExpires",
                DateTime.UtcNow
                    .AddMinutes(5)
                    .ToString("O"));

            HttpContext.Session.SetInt32(
                "Register_OtpAttempts",
                0);


            // =================================================
            // GỬI OTP QUA EMAIL
            // =================================================

            try
            {
                await _emailService.SendOtpAsync(
                    email,
                    otp);
            }
            catch
            {
                ClearRegisterSession();

                ViewBag.Error =
                    "Không thể gửi mã xác minh đến Email. " +
                    "Vui lòng kiểm tra Email và thử lại.";

                return View(model);
            }


            // =================================================
            // CHUYỂN SANG TRANG XÁC MINH
            // =================================================

            return RedirectToAction(
                nameof(XacMinhEmail));
        }

        // =====================================================
        // XÁC MINH EMAIL - HIỂN THỊ
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult XacMinhEmail()
        {
            string? pendingUserJson =
                HttpContext.Session.GetString(
                    "Register_PendingUser");

            if (string.IsNullOrWhiteSpace(
                pendingUserJson))
            {
                return RedirectToAction(
                    nameof(Register));
            }

            var user =
                System.Text.Json.JsonSerializer
                    .Deserialize<NguoiDung>(
                        pendingUserJson);

            if (user == null)
            {
                ClearRegisterSession();

                return RedirectToAction(
                    nameof(Register));
            }

            ViewBag.Email =
                user.Email;

            return View();
        }


        // =====================================================
        // XÁC MINH EMAIL - KIỂM TRA OTP
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacMinhEmail(
            RegisterVerifyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string? pendingUserJson =
                HttpContext.Session.GetString(
                    "Register_PendingUser");

            string? savedOtpHash =
                HttpContext.Session.GetString(
                    "Register_OtpHash");

            string? expiresString =
                HttpContext.Session.GetString(
                    "Register_OtpExpires");

            int attempts =
                HttpContext.Session.GetInt32(
                    "Register_OtpAttempts")
                ?? 0;


            if (string.IsNullOrWhiteSpace(
                    pendingUserJson) ||
                string.IsNullOrWhiteSpace(
                    savedOtpHash) ||
                string.IsNullOrWhiteSpace(
                    expiresString))
            {
                ModelState.AddModelError(
                    "",
                    "Phiên xác minh đã hết hạn. " +
                    "Vui lòng đăng ký lại.");

                return View(model);
            }


            if (attempts >= 5)
            {
                ClearRegisterSession();

                ModelState.AddModelError(
                    "",
                    "Bạn đã nhập sai OTP quá số lần cho phép. " +
                    "Vui lòng đăng ký lại.");

                return View(model);
            }


            if (!DateTime.TryParse(
                expiresString,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTime expiresAt))
            {
                ClearRegisterSession();

                ModelState.AddModelError(
                    "",
                    "Mã xác minh không hợp lệ.");

                return View(model);
            }


            if (DateTime.UtcNow > expiresAt)
            {
                ClearRegisterSession();

                ModelState.AddModelError(
                    "",
                    "Mã xác minh đã hết hạn. " +
                    "Vui lòng đăng ký lại.");

                return View(model);
            }


            attempts++;

            HttpContext.Session.SetInt32(
                "Register_OtpAttempts",
                attempts);


            string enteredOtpHash =
                HashOtp(
                    model.OTP.Trim());


            if (!string.Equals(
                enteredOtpHash,
                savedOtpHash,
                StringComparison.Ordinal))
            {
                ModelState.AddModelError(
                    "",
                    $"Mã OTP không đúng. " +
                    $"Bạn còn {5 - attempts} lần thử.");

                return View(model);
            }


            // =================================================
            // OTP ĐÚNG
            // =================================================

            var user =
                System.Text.Json.JsonSerializer
                    .Deserialize<NguoiDung>(
                        pendingUserJson);

            if (user == null)
            {
                ClearRegisterSession();

                ModelState.AddModelError(
                    "",
                    "Thông tin đăng ký không hợp lệ.");

                return View(model);
            }


            // =================================================
            // KIỂM TRA EMAIL LẦN CUỐI
            // =================================================

            bool existed =
                await _context.NguoiDungs
                    .AnyAsync(
                        x => x.Email == user.Email);

            if (existed)
            {
                ClearRegisterSession();

                ModelState.AddModelError(
                    "",
                    "Email này đã được đăng ký.");

                return View(model);
            }


            // =================================================
            // LƯU DATABASE
            // =================================================

            _context.NguoiDungs.Add(
                user);

            await _context.SaveChangesAsync();


            // =================================================
            // XÓA SESSION
            // =================================================

            ClearRegisterSession();


            TempData["Success"] =
                "Xác minh Email thành công! " +
                "Tài khoản đã được tạo. Vui lòng đăng nhập.";

            return RedirectToAction(
                nameof(Login));
        }

        // =====================================================
        // GỬI LẠI OTP ĐĂNG KÝ
        // =====================================================

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiLaiOTP()
        {
            string? pendingUserJson =
                HttpContext.Session.GetString(
                    "Register_PendingUser");

            if (string.IsNullOrWhiteSpace(
                pendingUserJson))
            {
                return RedirectToAction(
                    nameof(Register));
            }


            var user =
                System.Text.Json.JsonSerializer
                    .Deserialize<NguoiDung>(
                        pendingUserJson);

            if (user == null ||
                string.IsNullOrWhiteSpace(user.Email))
            {
                ClearRegisterSession();

                return RedirectToAction(
                    nameof(Register));
            }


            string otp =
                RandomNumberGenerator
                    .GetInt32(
                        100000,
                        1000000)
                    .ToString();

            string otpHash =
                HashOtp(otp);


            HttpContext.Session.SetString(
                "Register_OtpHash",
                otpHash);

            HttpContext.Session.SetString(
                "Register_OtpExpires",
                DateTime.UtcNow
                    .AddMinutes(5)
                    .ToString("O"));

            HttpContext.Session.SetInt32(
                "Register_OtpAttempts",
                0);


            try
            {
                await _emailService.SendOtpAsync(
                    user.Email,
                    otp);
            }
            catch
            {
                TempData["Error"] =
                    "Không thể gửi lại mã OTP.";

                return RedirectToAction(
                    nameof(XacMinhEmail));
            }


            TempData["Success"] =
                "Mã OTP mới đã được gửi đến Email của bạn.";

            return RedirectToAction(
                nameof(XacMinhEmail));
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