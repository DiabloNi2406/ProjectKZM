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
        private readonly ThongBaoService _thongBaoService;

        public ThanhToanController(
            MyDbContext context,
            IConfiguration configuration,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _configuration = configuration;
            _thongBaoService = thongBaoService;
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
        public async Task<IActionResult> DatHangCOD(
    ThanhToanViewModel model)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.SanPhams == null ||
                model.SanPhams.Count == 0)
            {
                TempData["Error"] =
                    "Không có sản phẩm nào được chọn.";

                return RedirectToAction(
                    "Index",
                    "GioHangs");
            }

            var gioHang =
                await _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(
                        g => g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null)
            {
                TempData["Error"] =
                    "Không tìm thấy giỏ hàng.";

                return RedirectToAction(
                    "Index",
                    "GioHangs");
            }

            var newProducts =
                new List<SanPhamThanhToanViewModel>();

            foreach (var item in model.SanPhams)
            {
                if (item.SoLuong <= 0)
                {
                    TempData["Error"] =
                        "Số lượng sản phẩm không hợp lệ.";

                    return RedirectToAction(
                        "Index",
                        "GioHangs");
                }

                var chiTiet =
                    gioHang.ChiTietGioHangs
                        .FirstOrDefault(
                            ct => ct.MaSach == item.MaSach);

                if (chiTiet == null ||
                    chiTiet.Sach == null)
                {
                    TempData["Error"] =
                        "Có sản phẩm không còn trong giỏ hàng.";

                    return RedirectToAction(
                        "Index",
                        "GioHangs");
                }

                if (item.SoLuong > chiTiet.SoLuong)
                {
                    TempData["Error"] =
                        $"Số lượng của \"{chiTiet.Sach.TenSach}\" " +
                        "vượt quá số lượng trong giỏ.";

                    return RedirectToAction(
                        "Index",
                        "GioHangs");
                }

                if (chiTiet.Sach.SoLuong < item.SoLuong)
                {
                    TempData["Error"] =
                        $"Sách \"{chiTiet.Sach.TenSach}\" " +
                        "không đủ tồn kho.";

                    return RedirectToAction(
                        "Index",
                        "GioHangs");
                }

                newProducts.Add(
                    new SanPhamThanhToanViewModel
                    {
                        MaSach = chiTiet.MaSach,
                        TenSach = chiTiet.Sach.TenSach,
                        Hinh = chiTiet.Sach.Hinh,
                        SoLuong = item.SoLuong,
                        DonGia = chiTiet.Sach.Gia
                    });
            }

            model.SanPhams = newProducts;

            model.TongSoLuong =
                model.SanPhams.Sum(x => x.SoLuong);

            model.TongTien =
                model.SanPhams.Sum(x => x.ThanhTien);

            model.PhuongThucThanhToan =
                "COD";

            return View(
                "~/Views/CashOnDelivery/Index.cshtml",
                model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanCOD(
    ThanhToanViewModel model)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.SanPhams == null ||
                model.SanPhams.Count == 0)
            {
                ViewData["Success"] = false;
                ViewData["Message"] =
                    "Không có sản phẩm nào để đặt hàng.";

                return View(
                    "~/Views/CashOnDelivery/KetQua.cshtml");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var gioHang = await _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(
                        g => g.MaNguoiDung == user.MaNguoiDung);

                if (gioHang == null)
                {
                    throw new Exception(
                        "Không tìm thấy giỏ hàng.");
                }

                decimal tongTien = 0;

                var chiTietDonHangMoi =
                    new List<ChiTietDonHang>();


                foreach (var item in model.SanPhams)
                {
                    var chiTietGio =
                        gioHang.ChiTietGioHangs
                            .FirstOrDefault(
                                ct => ct.MaSach == item.MaSach);

                    if (chiTietGio == null ||
                        chiTietGio.Sach == null)
                    {
                        throw new Exception(
                            "Có sản phẩm không còn trong giỏ hàng.");
                    }

                    if (item.SoLuong <= 0 ||
                        item.SoLuong > chiTietGio.SoLuong)
                    {
                        throw new Exception(
                            $"Số lượng của \"{chiTietGio.Sach.TenSach}\" không hợp lệ.");
                    }

                    if (chiTietGio.Sach.SoLuong < item.SoLuong)
                    {
                        throw new Exception(
                            $"Sách \"{chiTietGio.Sach.TenSach}\" không đủ tồn kho.");
                    }


                    decimal donGia =
                        chiTietGio.Sach.Gia;

                    decimal thanhTien =
                        donGia * item.SoLuong;

                    tongTien += thanhTien;


                    chiTietDonHangMoi.Add(
                        new ChiTietDonHang
                        {
                            MaSach = item.MaSach,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = thanhTien
                        });


                    // =====================================================
                    // GIỮ HÀNG
                    // =====================================================

                    chiTietGio.Sach.SoLuong -=
                        item.SoLuong;
                }


                var donHang = new DonHang
                {
                    NgayDat = DateTime.Now,

                    TongTien = tongTien,

                    TrangThai =
                        "Chờ Admin xác nhận",

                    PhuongThucThanhToan =
                        "COD",

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

                await _context.SaveChangesAsync();

                // =====================================================
                // TẠO THÔNG BÁO
                // =====================================================

                await _thongBaoService.CreateForAdminsAsync(
                    "Đơn hàng COD mới",
                    $"Đơn hàng #{donHang.MaDonHang} đang chờ Admin xác nhận.",
                    $"/DonHangs/ChiTiet/{donHang.MaDonHang}",
                    "COD",
                    donHang.MaDonHang);

                await transaction.CommitAsync();


                ViewData["Success"] = true;

                ViewData["Message"] =
                    $"Đặt hàng COD thành công. Đơn hàng #{donHang.MaDonHang} " +
                    "đang chờ Admin xác nhận. Sản phẩm đã được giữ cho bạn.";

                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                return View(
                    "~/Views/CashOnDelivery/KetQua.cshtml");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ViewData["Success"] = false;

                ViewData["Message"] =
                    ex.Message;

                return View(
                    "~/Views/CashOnDelivery/KetQua.cshtml");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHangChuyenKhoan(
    ThanhToanViewModel model)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.SanPhams == null ||
                model.SanPhams.Count == 0)
            {
                TempData["Error"] = "Không có sản phẩm nào được chọn.";
                return RedirectToAction("Index", "GioHangs");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var gioHang =
                    await _context.GioHangs
                        .Include(g => g.ChiTietGioHangs)
                            .ThenInclude(ct => ct.Sach)
                        .FirstOrDefaultAsync(
                            g => g.MaNguoiDung == user.MaNguoiDung);

                if (gioHang == null)
                {
                    throw new Exception("Không tìm thấy giỏ hàng.");
                }

                decimal tongTien = 0;

                var chiTietDonHangMoi =
                    new List<ChiTietDonHang>();

                foreach (var item in model.SanPhams)
                {
                    var chiTietGio =
                        gioHang.ChiTietGioHangs
                            .FirstOrDefault(ct =>
                                ct.MaSach == item.MaSach);

                    if (chiTietGio == null ||
                        chiTietGio.Sach == null)
                    {
                        throw new Exception(
                            "Có sản phẩm không còn trong giỏ hàng.");
                    }

                    if (item.SoLuong <= 0 ||
                        item.SoLuong > chiTietGio.SoLuong)
                    {
                        throw new Exception(
                            $"Số lượng của \"{chiTietGio.Sach.TenSach}\" không hợp lệ.");
                    }

                    if (chiTietGio.Sach.SoLuong < item.SoLuong)
                    {
                        throw new Exception(
                            $"Sách \"{chiTietGio.Sach.TenSach}\" không đủ tồn kho.");
                    }

                    decimal donGia = chiTietGio.Sach.Gia;
                    decimal thanhTien = donGia * item.SoLuong;

                    tongTien += thanhTien;

                    chiTietDonHangMoi.Add(
                        new ChiTietDonHang
                        {
                            MaSach = item.MaSach,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = thanhTien
                        });

                    // GIỮ HÀNG
                    chiTietGio.Sach.SoLuong -= item.SoLuong;
                }

                var donHang = new DonHang
                {
                    NgayDat = DateTime.Now,
                    TongTien = tongTien,
                    TrangThai = "Chờ Admin xác nhận",
                    PhuongThucThanhToan = "BANK",
                    DiaChiGiaoHang = model.DiaChiGiaoHang.Trim(),
                    MaNguoiDung = user.MaNguoiDung,
                    GhiChu = string.IsNullOrWhiteSpace(model.GhiChu)
                        ? null
                        : model.GhiChu.Trim(),
                    ChiTietDonHangs = chiTietDonHangMoi
                };

                _context.DonHangs.Add(donHang);

                await _context.SaveChangesAsync();

                // =====================================================
                // TẠO THÔNG BÁO
                // =====================================================

                await _thongBaoService.CreateForAdminsAsync(
                    "Đơn chuyển khoản mới",
                    $"Đơn hàng #{donHang.MaDonHang} đang chờ Admin kiểm tra chuyển khoản.",
                    $"/DonHangs/ChiTiet/{donHang.MaDonHang}",
                    "BANK",
                    donHang.MaDonHang);

                await transaction.CommitAsync();

                // Lúc này MaDonHang đã được SQL Server sinh
                model.MaDonHang = donHang.MaDonHang;

                model.PhuongThucThanhToan = "BANK";

                // Dựng lại dữ liệu hiển thị
                var sanPhams = new List<SanPhamThanhToanViewModel>();

                foreach (var item in chiTietDonHangMoi)
                {
                    var sach = await _context.Sachs
                        .FirstAsync(s => s.MaSach == item.MaSach);

                    sanPhams.Add(
                        new SanPhamThanhToanViewModel
                        {
                            MaSach = sach.MaSach,
                            TenSach = sach.TenSach,
                            Hinh = sach.Hinh,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        });
                }

                model.SanPhams = sanPhams;
                model.TongSoLuong = sanPhams.Sum(x => x.SoLuong);
                model.TongTien = sanPhams.Sum(x => x.ThanhTien);

                return View(
                    "~/Views/ChuyenKhoan/Index.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] = ex.Message;

                return RedirectToAction("Index", "GioHangs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanChuyenKhoan(
    ThanhToanViewModel model)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.SanPhams == null ||
                model.SanPhams.Count == 0)
            {
                ViewData["Success"] = false;
                ViewData["Message"] =
                    "Không có sản phẩm nào để đặt hàng.";

                return View(
                    "~/Views/ChuyenKhoan/KetQua.cshtml");
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var gioHang = await _context.GioHangs
                    .Include(g => g.ChiTietGioHangs)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(
                        g => g.MaNguoiDung == user.MaNguoiDung);

                if (gioHang == null)
                {
                    throw new Exception(
                        "Không tìm thấy giỏ hàng.");
                }

                decimal tongTien = 0;

                var chiTietDonHangMoi =
                    new List<ChiTietDonHang>();


                foreach (var item in model.SanPhams)
                {
                    var chiTietGio =
                        gioHang.ChiTietGioHangs
                            .FirstOrDefault(
                                ct => ct.MaSach == item.MaSach);

                    if (chiTietGio == null ||
                        chiTietGio.Sach == null)
                    {
                        throw new Exception(
                            "Có sản phẩm không còn trong giỏ hàng.");
                    }

                    if (item.SoLuong <= 0 ||
                        item.SoLuong > chiTietGio.SoLuong)
                    {
                        throw new Exception(
                            $"Số lượng của \"{chiTietGio.Sach.TenSach}\" không hợp lệ.");
                    }

                    if (chiTietGio.Sach.SoLuong < item.SoLuong)
                    {
                        throw new Exception(
                            $"Sách \"{chiTietGio.Sach.TenSach}\" không đủ tồn kho.");
                    }


                    decimal donGia =
                        chiTietGio.Sach.Gia;

                    decimal thanhTien =
                        donGia * item.SoLuong;

                    tongTien += thanhTien;


                    chiTietDonHangMoi.Add(
                        new ChiTietDonHang
                        {
                            MaSach = item.MaSach,
                            SoLuong = item.SoLuong,
                            DonGia = donGia,
                            ThanhTien = thanhTien
                        });


                    // =====================================================
                    // GIỮ HÀNG
                    // =====================================================

                    chiTietGio.Sach.SoLuong -=
                        item.SoLuong;
                }


                var donHang = new DonHang
                {
                    NgayDat = DateTime.Now,

                    TongTien = tongTien,

                    TrangThai =
                        "Chờ Admin xác nhận",

                    PhuongThucThanhToan =
                        "BANK",

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

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                ViewData["Success"] = true;

                ViewData["Message"] =
                    $"Đã ghi nhận yêu cầu chuyển khoản cho đơn #{donHang.MaDonHang}. " +
                    "Sản phẩm đã được giữ và đang chờ Admin kiểm tra giao dịch.";

                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                return View(
                    "~/Views/ChuyenKhoan/KetQua.cshtml");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                ViewData["Success"] = false;

                ViewData["Message"] =
                    ex.Message;

                return View(
                    "~/Views/ChuyenKhoan/KetQua.cshtml");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatHangVNPAY(
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
            // =====================================================
            // LẤY CẤU HÌNH VNPAY
            // =====================================================

            var settings =
                _configuration
                    .GetSection("VNPay")
                    .Get<VNPaySettings>();


            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.HashSecret))
            {
                ViewData["Success"] = false;
                ViewData["Message"] =
                    "Không tìm thấy cấu hình VNPAY.";

                return View("KetQua");
            }


            // =====================================================
            // ĐỌC DỮ LIỆU VNPAY TRẢ VỀ
            // =====================================================

            var vnpay =
                new VnPayLibrary();


            foreach (var (key, value) in Request.Query)
            {
                if (!string.IsNullOrEmpty(key) &&
                    key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(
                        key,
                        value.ToString());
                }
            }


            // =====================================================
            // KIỂM TRA CHỮ KÝ
            // PHẢI KIỂM TRA TRƯỚC KHI XỬ LÝ ĐƠN HÀNG
            // =====================================================

            string vnpSecureHash =
                Request.Query["vnp_SecureHash"].ToString();


            bool hashValid =
                vnpay.ValidateSignature(
                    vnpSecureHash,
                    settings.HashSecret);


            if (!hashValid)
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Chữ ký giao dịch không hợp lệ.";

                return View("KetQua");
            }


            // =====================================================
            // LẤY DỮ LIỆU GIAO DỊCH
            // =====================================================

            string txnRef =
                vnpay.GetResponseData(
                    "vnp_TxnRef");

            string responseCode =
                vnpay.GetResponseData(
                    "vnp_ResponseCode");

            string transactionStatus =
                vnpay.GetResponseData(
                    "vnp_TransactionStatus");

            string transactionNo =
                vnpay.GetResponseData(
                    "vnp_TransactionNo");

            string amountStr =
                vnpay.GetResponseData(
                    "vnp_Amount");

            string tmnCode =
                vnpay.GetResponseData(
                    "vnp_TmnCode");


            // =====================================================
            // KIỂM TRA TMN CODE
            // =====================================================

            if (!string.Equals(
                    tmnCode,
                    settings.TmnCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Mã website VNPAY không hợp lệ.";

                return View("KetQua");
            }


            // =====================================================
            // KIỂM TRA MÃ ĐƠN
            // =====================================================

            if (!int.TryParse(
                    txnRef,
                    out int maDonHang))
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Mã đơn hàng không hợp lệ.";

                return View("KetQua");
            }


            // =====================================================
            // KIỂM TRA SỐ TIỀN
            // =====================================================

            if (!long.TryParse(
                    amountStr,
                    out long vnpAmount))
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Số tiền giao dịch không hợp lệ.";

                return View("KetQua");
            }


            // =====================================================
            // LẤY ĐƠN HÀNG
            // =====================================================

            var donHang =
                await _context.DonHangs
                    .Include(d =>
                        d.ChiTietDonHangs)
                        .ThenInclude(ct =>
                            ct.Sach)
                    .FirstOrDefaultAsync(
                        d =>
                            d.MaDonHang ==
                            maDonHang);


            if (donHang == null)
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Không tìm thấy đơn hàng.";

                return View("KetQua");
            }


            // =====================================================
            // KIỂM TRA SỐ TIỀN
            // =====================================================

            long expectedAmount =
                Convert.ToInt64(
                    Math.Round(
                        donHang.TongTien * 100m,
                        MidpointRounding.AwayFromZero));


            if (vnpAmount != expectedAmount)
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Số tiền giao dịch không khớp.";

                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                return View("KetQua");
            }


            // =====================================================
            // KIỂM TRA PHƯƠNG THỨC
            // =====================================================

            if (!string.Equals(
                    donHang.PhuongThucThanhToan,
                    "VNPAY",
                    StringComparison.OrdinalIgnoreCase))
            {
                ViewData["Success"] = false;

                ViewData["Message"] =
                    "Phương thức thanh toán của đơn hàng không hợp lệ.";

                return View("KetQua");
            }


            // =====================================================
            // KIỂM TRA GIAO DỊCH THÀNH CÔNG
            // =====================================================

            bool thanhToanThanhCong =
                responseCode == "00" &&
                transactionStatus == "00";


            // =====================================================
            // THANH TOÁN THẤT BẠI
            // =====================================================

            if (!thanhToanThanhCong)
            {
                donHang.TrangThai =
                    "Thanh toán thất bại";

                await _context.SaveChangesAsync();


                ViewData["Success"] =
                    false;

                ViewData["Message"] =
                    "Thanh toán VNPAY thất bại.";


                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                ViewData["TransactionNo"] =
                    transactionNo;


                return View("KetQua");
            }


            // =====================================================
            // CHỐNG XỬ LÝ ĐƠN HAI LẦN
            // =====================================================

            if (donHang.TrangThai ==
                "Đã thanh toán")
            {
                ViewData["Success"] =
                    true;

                ViewData["Message"] =
                    "Đơn hàng đã được thanh toán trước đó.";


                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                ViewData["TransactionNo"] =
                    transactionNo;

                ViewData["TongTien"] =
                    donHang.TongTien.ToString("N0");


                return View("KetQua");
            }


            // =====================================================
            // TRANSACTION DB
            // =====================================================

            using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =================================================
                // LẤY GIỎ HÀNG CỦA USER
                // =================================================

                var gioHang =
                    await _context.GioHangs
                        .Include(g =>
                            g.ChiTietGioHangs)
                        .FirstOrDefaultAsync(
                            g =>
                                g.MaNguoiDung ==
                                donHang.MaNguoiDung);


                if (gioHang == null)
                {
                    throw new Exception(
                        "Không tìm thấy giỏ hàng của người dùng.");
                }


                // =================================================
                // KIỂM TRA + TRỪ TỒN KHO
                // =================================================

                foreach (
                    var chiTiet
                    in donHang.ChiTietDonHangs)
                {
                    if (chiTiet.Sach == null)
                    {
                        throw new Exception(
                            $"Không tìm thấy sách #{chiTiet.MaSach}.");
                    }


                    // ---------------------------------------------
                    // KIỂM TRA TỒN KHO LẦN CUỐI
                    // ---------------------------------------------

                    if (chiTiet.Sach.SoLuong <
                        chiTiet.SoLuong)
                    {
                        throw new Exception(
                            $"Sách \"{chiTiet.Sach.TenSach}\" " +
                            "không đủ số lượng tồn kho tại thời điểm thanh toán.");
                    }


                    // ---------------------------------------------
                    // TRỪ TỒN KHO
                    // ---------------------------------------------

                    chiTiet.Sach.SoLuong -=
                        chiTiet.SoLuong;
                }


                // =================================================
                // GIẢM / XÓA SẢN PHẨM KHỎI GIỎ
                // =================================================

                foreach (
                    var chiTietDon
                    in donHang.ChiTietDonHangs)
                {
                    var chiTietGio =
                        gioHang.ChiTietGioHangs
                            .FirstOrDefault(
                                x =>
                                    x.MaSach ==
                                    chiTietDon.MaSach);


                    if (chiTietGio == null)
                    {
                        continue;
                    }


                    // ---------------------------------------------
                    // THANH TOÁN HẾT SỐ LƯỢNG TRONG GIỎ
                    // ---------------------------------------------

                    if (chiTietGio.SoLuong <=
                        chiTietDon.SoLuong)
                    {
                        _context.ChiTietGioHangs.Remove(
                            chiTietGio);
                    }
                    else
                    {
                        // -----------------------------------------
                        // CHỈ THANH TOÁN MỘT PHẦN
                        // -----------------------------------------

                        chiTietGio.SoLuong -=
                            chiTietDon.SoLuong;
                    }
                }


                // =================================================
                // CHỐT ĐƠN
                // =================================================

                donHang.TrangThai =
                    "Đã thanh toán";

                donHang.PhuongThucThanhToan =
                    "VNPAY";


                // =================================================
                // SAVE
                // =================================================

                await _context.SaveChangesAsync();


            

                // =================================================
                // CHỐT ĐƠN
                // =================================================

                donHang.TrangThai =
                    "Đã thanh toán";

                donHang.PhuongThucThanhToan =
                    "VNPAY";

                // =================================================
                // SAVE
                // =================================================

                await _context.SaveChangesAsync();

                // =================================================
                // COMMIT
                // =================================================

                await transaction.CommitAsync();



                // =================================================
                // TẠO THÔNG BÁO CHO ADMIN
                // CHỈ KHI VNPAY THANH TOÁN THÀNH CÔNG
                // =================================================

                await _thongBaoService.CreateForAdminsAsync(
                    "Đơn hàng VNPAY mới",
                    $"Đơn hàng #{donHang.MaDonHang} đã thanh toán thành công qua VNPAY.",
                    $"/DonHangs/ChiTiet/{donHang.MaDonHang}",
                    "VNPAY",
                    donHang.MaDonHang);


                // =================================================
                // KẾT QUẢ
                // =================================================

                ViewData["Success"] =
                    true;

                ViewData["Message"] =
                    "Thanh toán VNPAY thành công. " +
                    "Đơn hàng đã được xác nhận và hàng đã được trừ khỏi kho.";


                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                ViewData["TransactionNo"] =
                    transactionNo;

                ViewData["TongTien"] =
                    donHang.TongTien.ToString("N0");


                return View("KetQua");
            }
            catch (Exception ex)
            {
                // =================================================
                // ROLLBACK
                // =================================================

                await transaction.RollbackAsync();


                ViewData["Success"] =
                    false;

                ViewData["Message"] =
                    "Thanh toán VNPAY đã thành công nhưng " +
                    "hệ thống chưa thể hoàn tất đơn hàng.";


                ViewData["MaDonHang"] =
                    donHang.MaDonHang;

                ViewData["TransactionNo"] =
                    transactionNo;


                ViewData["ErrorDetail"] =
                    ex.Message;


                return View("KetQua");
            }
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