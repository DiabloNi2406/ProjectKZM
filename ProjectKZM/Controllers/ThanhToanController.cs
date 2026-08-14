using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.Services;
using ProjectKZM.ViewModels;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class ThanhToanController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IConfiguration _configuration;

        public ThanhToanController(
            MyDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // =========================================================
        // LẤY USER ĐANG ĐĂNG NHẬP
        // =========================================================

        private async Task<NguoiDung?> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.Email == email);
        }


        // =========================================================
        // INDEX - TRANG THANH TOÁN
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int[] maSach,
            int[] soLuong)
        {
            // -----------------------------------------------------
            // KIỂM TRA DỮ LIỆU
            // -----------------------------------------------------

            if (maSach == null ||
                soLuong == null ||
                maSach.Length == 0 ||
                soLuong.Length == 0 ||
                maSach.Length != soLuong.Length)
            {
                TempData["Error"] =
                    "Không có sản phẩm nào được chọn để thanh toán.";

                return RedirectToAction("Index", "GioHangs");
            }


            // -----------------------------------------------------
            // USER
            // -----------------------------------------------------

            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            // -----------------------------------------------------
            // GIỎ HÀNG
            // -----------------------------------------------------

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);


            if (gioHang == null)
            {
                TempData["Error"] =
                    "Không tìm thấy giỏ hàng.";

                return RedirectToAction("Index", "GioHangs");
            }


            // -----------------------------------------------------
            // VIEW MODEL
            // -----------------------------------------------------

            var model = new ThanhToanViewModel
            {
                HoTen = user.HoTen,
                SoDienThoai = user.SoDienThoai ?? string.Empty,
                DiaChiGiaoHang = user.DiaChi ?? string.Empty,
                PhuongThucThanhToan = "VNPAY"
            };


            // -----------------------------------------------------
            // LẤY SẢN PHẨM ĐƯỢC CHỌN
            // -----------------------------------------------------

            for (int i = 0; i < maSach.Length; i++)
            {
                int idSach = maSach[i];
                int qty = soLuong[i];


                if (qty <= 0)
                {
                    TempData["Error"] =
                        "Số lượng sản phẩm không hợp lệ.";

                    return RedirectToAction("Index", "GioHangs");
                }


                var chiTiet = gioHang.ChiTietGioHangs
                    .FirstOrDefault(
                        ct => ct.MaSach == idSach);


                if (chiTiet == null ||
                    chiTiet.Sach == null)
                {
                    TempData["Error"] =
                        "Có sản phẩm không tồn tại trong giỏ hàng.";

                    return RedirectToAction("Index", "GioHangs");
                }


                // -------------------------------------------------
                // KHÔNG CHO VƯỢT QUÁ SỐ LƯỢNG TRONG GIỎ
                // -------------------------------------------------

                if (qty > chiTiet.SoLuong)
                {
                    TempData["Error"] =
                        $"Số lượng thanh toán của \"{chiTiet.Sach.TenSach}\" " +
                        "vượt quá số lượng trong giỏ.";

                    return RedirectToAction("Index", "GioHangs");
                }


                // -------------------------------------------------
                // KIỂM TRA TỒN KHO
                // -------------------------------------------------

                if (chiTiet.Sach.SoLuong < qty)
                {
                    TempData["Error"] =
                        $"Sách \"{chiTiet.Sach.TenSach}\" " +
                        "không đủ số lượng trong kho.";

                    return RedirectToAction("Index", "GioHangs");
                }


                // -------------------------------------------------
                // THÊM VÀO VIEW MODEL
                // -------------------------------------------------

                model.SanPhams.Add(
                    new SanPhamThanhToanViewModel
                    {
                        MaSach = chiTiet.MaSach,
                        TenSach = chiTiet.Sach.TenSach,
                        Hinh = chiTiet.Sach.Hinh,
                        SoLuong = qty,
                        DonGia = chiTiet.Sach.Gia
                    });
            }


            // -----------------------------------------------------
            // TÍNH TỔNG
            // -----------------------------------------------------

            model.TongSoLuong =
                model.SanPhams.Sum(x => x.SoLuong);

            model.TongTien =
                model.SanPhams.Sum(x => x.ThanhTien);


            return View(model);
        }


        // =========================================================
        // ĐẶT HÀNG
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHang(
            ThanhToanViewModel model)
        {
            // -----------------------------------------------------
            // USER
            // -----------------------------------------------------

            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            // -----------------------------------------------------
            // CHỈ CHO VNPAY
            // -----------------------------------------------------

            model.PhuongThucThanhToan = "VNPAY";


            // -----------------------------------------------------
            // KIỂM TRA MODEL
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }


            if (model.SanPhams == null ||
                model.SanPhams.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Không có sản phẩm nào được chọn.");

                return View("Index", model);
            }


            // -----------------------------------------------------
            // LẤY GIỎ HÀNG
            // -----------------------------------------------------

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);


            if (gioHang == null)
            {
                ModelState.AddModelError(
                    "",
                    "Không tìm thấy giỏ hàng.");

                return View("Index", model);
            }


            // -----------------------------------------------------
            // TẠO DONHANG TẠM
            // -----------------------------------------------------

            decimal tongTien = 0;
            int tongSoLuong = 0;

            var chiTietDonHangMoi =
                new List<ChiTietDonHang>();


            foreach (var item in model.SanPhams)
            {
                if (item.SoLuong <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        "Số lượng sản phẩm không hợp lệ.");

                    return View("Index", model);
                }


                var chiTietGio =
                    gioHang.ChiTietGioHangs
                        .FirstOrDefault(
                            ct => ct.MaSach == item.MaSach);


                if (chiTietGio == null ||
                    chiTietGio.Sach == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Có sản phẩm không còn trong giỏ hàng.");

                    return View("Index", model);
                }


                // -------------------------------------------------
                // KHÔNG ĐƯỢC VƯỢT SỐ LƯỢNG TRONG GIỎ
                // -------------------------------------------------

                if (item.SoLuong > chiTietGio.SoLuong)
                {
                    ModelState.AddModelError(
                        "",
                        $"Số lượng thanh toán của " +
                        $"\"{chiTietGio.Sach.TenSach}\" vượt quá giỏ hàng.");

                    return View("Index", model);
                }


                // -------------------------------------------------
                // KIỂM TRA TỒN KHO LẦN CUỐI
                // -------------------------------------------------

                if (chiTietGio.Sach.SoLuong < item.SoLuong)
                {
                    ModelState.AddModelError(
                        "",
                        $"Sách \"{chiTietGio.Sach.TenSach}\" " +
                        "không đủ số lượng trong kho.");

                    return View("Index", model);
                }


                // -------------------------------------------------
                // LẤY GIÁ TRỰC TIẾP TỪ DATABASE
                // KHÔNG TIN DonGia TỪ FORM
                // -------------------------------------------------

                decimal donGia =
                    chiTietGio.Sach.Gia;

                decimal thanhTien =
                    donGia * item.SoLuong;


                tongTien += thanhTien;
                tongSoLuong += item.SoLuong;


                chiTietDonHangMoi.Add(
                    new ChiTietDonHang
                    {
                        //MaDonHang = 0,
                        MaSach = item.MaSach,
                        SoLuong = item.SoLuong,
                        DonGia = donGia,
                        ThanhTien = donGia * item.SoLuong
                    });
            }


            // -----------------------------------------------------
            // TẠO DONHANG
            // -----------------------------------------------------

            var donHang = new DonHang
            {
                NgayDat = DateTime.Now,

                TongTien = tongTien,

                TrangThai = "Chờ thanh toán",

                PhuongThucThanhToan = "VNPAY",

                DiaChiGiaoHang =
                    model.DiaChiGiaoHang.Trim(),

                MaNguoiDung =
                    user.MaNguoiDung,

                GhiChu =
                    string.IsNullOrWhiteSpace(model.GhiChu)
                        ? null
                        : model.GhiChu.Trim(),

                ChiTietDonHangs =
                    chiTietDonHangMoi
            };


            _context.DonHangs.Add(donHang);


            try
            {
                // -------------------------------------------------
                // LƯU ĐỂ LẤY MaDonHang
                // -------------------------------------------------

                await _context.SaveChangesAsync();


                // -------------------------------------------------
                // TẠO URL VNPAY
                // -------------------------------------------------

                string paymentUrl =
                    CreateVNPayPaymentUrl(
                        donHang,
                        user,
                        HttpContext.Connection.RemoteIpAddress?
                            .ToString()
                        ?? "127.0.0.1");


                // -------------------------------------------------
                // CHUYỂN SANG VNPAY
                // -------------------------------------------------

                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;

                if (ex.InnerException != null)
                {
                    errorMessage +=
                        " | INNER: " + ex.InnerException.Message;
                }

                if (ex.InnerException?.InnerException != null)
                {
                    errorMessage +=
                        " | INNER 2: " +
                        ex.InnerException.InnerException.Message;
                }

                TempData["Error"] = errorMessage;

                return RedirectToAction(
                    "Index",
                    "GioHangs");
            }
        }


        // =========================================================
        // VNPAY RETURN
        // =========================================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayReturn()
        {
            var settings = _configuration.GetSection("VNPay").Get<VNPaySettings>();
            if (settings == null || string.IsNullOrWhiteSpace(settings.HashSecret))
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Không tìm thấy cấu hình VNPAY.";
                return View("KetQua");
            }

            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            string vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString();
            bool hashValid = vnpay.ValidateSignature(vnp_SecureHash, settings.HashSecret);

            if (!hashValid)
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Chữ ký giao dịch không hợp lệ (Lỗi 70).";
                return View("KetQua");
            }

            string txnRef = vnpay.GetResponseData("vnp_TxnRef");
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            string transactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            string amountStr = vnpay.GetResponseData("vnp_Amount");

            if (!int.TryParse(txnRef, out int maDonHang))
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Mã đơn hàng không hợp lệ.";
                return View("KetQua");
            }

            if (!long.TryParse(amountStr, out long vnpAmount))
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Số tiền trả về không hợp lệ.";
                return View("KetQua");
            }

            // Load order
            var donHang = await _context.DonHangs
                .FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);

            if (donHang == null)
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Không tìm thấy đơn hàng.";
                return View("KetQua");
            }

            // Compare amounts (vnp_Amount is sent in smallest currency unit, e.g. cents)
            long expectedAmount = Convert.ToInt64(Math.Round(donHang.TongTien * 100m, MidpointRounding.AwayFromZero));
            if (vnpAmount != expectedAmount)
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "Số tiền giao dịch không khớp.";
                return View("KetQua");
            }

            // Minimal compile-time fix: ensure a result is returned.
            // You should replace the following default behavior with real handling:
            //  - load the DonHang by maDonHang,
            //  - compare vnp_Amount (divide by 100) with DonHang.TongTien,
            //  - update DonHang.TrangThai based on responseCode/transactionStatus,
            //  - save changes and show meaningful result to user.

            ViewData["Success"] = true;
            ViewData["Message"] = "Giao dịch đã được nhận bởi hệ thống. Vui lòng kiểm tra trạng thái đơn hàng.";
            return View("KetQua");
        }


        // =========================================================
        // TẠO URL VNPAY
        // =========================================================

        private string CreateVNPayPaymentUrl(
    DonHang donHang,
    NguoiDung user,
    string ipAddress)
        {
            var settings = _configuration.GetSection("VNPay").Get<VNPaySettings>();
            if (settings == null)
            {
                throw new Exception("Không tìm thấy cấu hình VNPay.");
            }

            TimeZoneInfo vietnamTimeZone;
            try
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }

            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            long amount = Convert.ToInt64(Math.Round(donHang.TongTien * 100m, MidpointRounding.AwayFromZero));

            var vnpay = new VnPayLibrary();

            vnpay.AddRequestData("vnp_Version", settings.Version);
            vnpay.AddRequestData("vnp_Command", settings.Command);
            vnpay.AddRequestData("vnp_TmnCode", settings.TmnCode);
            vnpay.AddRequestData("vnp_Amount", amount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", settings.CurrCode ?? "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress);
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {donHang.MaDonHang}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", settings.ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", donHang.MaDonHang.ToString());
            vnpay.AddRequestData("vnp_ExpireDate", now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

            return vnpay.CreateRequestUrl(settings.BaseUrl, settings.HashSecret);
        }


        // =========================================================
        // HMAC SHA512
        // =========================================================

        private static string HmacSha512(
            string key,
            string data)
        {
            using var hmac =
                new HMACSHA512(
                    Encoding.UTF8.GetBytes(key));

            byte[] hash =
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(data));

            return
                Convert.ToHexString(hash)
                    .ToLowerInvariant();
        }


        // =========================================================
        // URL ENCODE
        // =========================================================

        private static string UrlEncode(
    string value)
        {
            return HttpUtility.UrlEncode(
                value)
                ?? string.Empty;
        }
    }
}