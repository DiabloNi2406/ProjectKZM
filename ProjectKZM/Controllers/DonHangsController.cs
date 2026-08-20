using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.ViewModels;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class DonHangsController : Controller
    {
        private readonly MyDbContext _context;

        public DonHangsController(MyDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // LẤY USER ĐANG ĐĂNG NHẬP
        // =========================================================

        private async Task<NguoiDung?> GetCurrentUser()
        {
            var email =
                User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _context.NguoiDungs
                .FirstOrDefaultAsync(
                    x => x.Email == email);
        }


        // =========================================================
        // ADMIN - TẤT CẢ ĐƠN HÀNG
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders =
                await _context.DonHangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.ChiTietDonHangs)
                    .AsNoTracking()
                    .OrderByDescending(d => d.NgayDat)
                    .ToListAsync();


            var model =
                orders.Select(
                    d => new DonHangViewModel
                    {
                        MaDonHang = d.MaDonHang,

                        NgayDat = d.NgayDat,

                        TongTien = d.TongTien,

                        TrangThai =
                            d.TrangThai,

                        PhuongThucThanhToan =
                            d.PhuongThucThanhToan,

                        DiaChiGiaoHang =
                            d.DiaChiGiaoHang,

                        GhiChu =
                            d.GhiChu,

                        MaNguoiDung =
                            d.MaNguoiDung,

                        TenKhachHang =
                            d.NguoiDung?.HoTen
                            ?? "Không xác định",

                        EmailKhachHang =
                            d.NguoiDung?.Email
                            ?? string.Empty,

                        TongSoLuong =
                            d.ChiTietDonHangs
                                .Sum(x => x.SoLuong)
                    })
                    .ToList();


            return View(model);
        }


        // =========================================================
        // ADMIN - ĐƠN CHỜ XÁC NHẬN
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ChoXacNhan()
        {
            var orders =
                await _context.DonHangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.ChiTietDonHangs)
                    .Where(d =>
                        d.TrangThai ==
                        "Chờ Admin xác nhận")
                    .AsNoTracking()
                    .OrderBy(d => d.NgayDat)
                    .ToListAsync();


            var model =
                orders.Select(
                    d => new DonHangViewModel
                    {
                        MaDonHang = d.MaDonHang,

                        NgayDat = d.NgayDat,

                        TongTien = d.TongTien,

                        TrangThai =
                            d.TrangThai,

                        PhuongThucThanhToan =
                            d.PhuongThucThanhToan,

                        DiaChiGiaoHang =
                            d.DiaChiGiaoHang,

                        GhiChu =
                            d.GhiChu,

                        MaNguoiDung =
                            d.MaNguoiDung,

                        TenKhachHang =
                            d.NguoiDung?.HoTen
                            ?? "Không xác định",

                        EmailKhachHang =
                            d.NguoiDung?.Email
                            ?? string.Empty,

                        TongSoLuong =
                            d.ChiTietDonHangs
                                .Sum(x => x.SoLuong)
                    })
                    .ToList();


            return View(model);
        }


        // =========================================================
        // CHI TIẾT ĐƠN - ADMIN
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var donHang =
                await _context.DonHangs
                    .Include(d => d.NguoiDung)
                    .Include(d => d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.Sach)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        d => d.MaDonHang == id);


            if (donHang == null)
            {
                return NotFound();
            }


            var model =
                new DonHangChiTietViewModel
                {
                    MaDonHang =
                        donHang.MaDonHang,

                    NgayDat =
                        donHang.NgayDat,

                    TongTien =
                        donHang.TongTien,

                    TrangThai =
                        donHang.TrangThai,

                    PhuongThucThanhToan =
                        donHang.PhuongThucThanhToan,

                    DiaChiGiaoHang =
                        donHang.DiaChiGiaoHang,

                    GhiChu =
                        donHang.GhiChu,

                    TenKhachHang =
                        donHang.NguoiDung?.HoTen
                        ?? "Không xác định",

                    EmailKhachHang =
                        donHang.NguoiDung?.Email
                        ?? string.Empty
                };


            foreach (
                var ct in
                donHang.ChiTietDonHangs)
            {
                model.SanPhams.Add(
                    new DonHangSanPhamViewModel
                    {
                        MaSach =
                            ct.MaSach,

                        TenSach =
                            ct.Sach?.TenSach
                            ?? "Không xác định",

                        Hinh =
                            ct.Sach?.Hinh,

                        SoLuong =
                            ct.SoLuong,

                        DonGia =
                            ct.DonGia,

                        ThanhTien =
                            ct.ThanhTien
                    });
            }


            return View(model);
        }


        // =========================================================
        // ADMIN - XÁC NHẬN ĐƠN
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(
            int id)
        {
            using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var donHang =
                    await _context.DonHangs
                        .FirstOrDefaultAsync(
                            d => d.MaDonHang == id);


                if (donHang == null)
                {
                    return NotFound();
                }


                // -------------------------------------------------
                // CHỈ XÁC NHẬN ĐƠN ĐANG PENDING
                // -------------------------------------------------

                if (donHang.TrangThai !=
                    "Chờ Admin xác nhận")
                {
                    TempData["Error"] =
                        "Đơn hàng không còn ở trạng thái chờ xác nhận.";

                    return RedirectToAction(
                        nameof(ChoXacNhan));
                }


                // -------------------------------------------------
                // KHÔNG TRỪ KHO THÊM
                //
                // Vì COD/BANK đã giữ hàng khi đặt.
                // -------------------------------------------------

                donHang.TrangThai =
                    "Đã xác nhận";


                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                TempData["Success"] =
                    $"Đã xác nhận đơn hàng #{id}.";


                return RedirectToAction(
                    nameof(ChoXacNhan));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể xác nhận đơn hàng.";

                return RedirectToAction(
                    nameof(ChoXacNhan));
            }
        }


        // =========================================================
        // ADMIN - HỦY ĐƠN + TRẢ HÀNG VỀ KHO
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(
            int id)
        {
            using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var donHang =
                    await _context.DonHangs
                        .Include(d =>
                            d.ChiTietDonHangs)
                        .ThenInclude(ct => ct.Sach)
                        .FirstOrDefaultAsync(
                            d => d.MaDonHang == id);


                if (donHang == null)
                {
                    return NotFound();
                }


                // -------------------------------------------------
                // CHỈ HỦY ĐƠN PENDING
                // -------------------------------------------------

                if (donHang.TrangThai !=
                    "Chờ Admin xác nhận")
                {
                    TempData["Error"] =
                        "Chỉ có thể hủy đơn đang chờ xác nhận.";

                    return RedirectToAction(
                        nameof(ChoXacNhan));
                }


                // -------------------------------------------------
                // TRẢ LẠI HÀNG ĐÃ GIỮ
                // -------------------------------------------------

                foreach (
                    var chiTiet
                    in donHang.ChiTietDonHangs)
                {
                    if (chiTiet.Sach == null)
                    {
                        throw new Exception(
                            $"Không tìm thấy sách #{chiTiet.MaSach}.");
                    }


                    chiTiet.Sach.SoLuong +=
                        chiTiet.SoLuong;
                }


                // -------------------------------------------------
                // ĐÁNH DẤU HỦY
                // -------------------------------------------------

                donHang.TrangThai =
                    "Đã hủy";


                await _context.SaveChangesAsync();

                await transaction.CommitAsync();


                TempData["Success"] =
                    $"Đã hủy đơn hàng #{id} và trả hàng về kho.";


                return RedirectToAction(
                    nameof(ChoXacNhan));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "Không thể hủy đơn hàng.";

                return RedirectToAction(
                    nameof(ChoXacNhan));
            }
        }


        // =========================================================
        // USER - LỊCH SỬ ĐƠN HÀNG
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> LichSu()
        {
            var user =
                await GetCurrentUser();


            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var orders =
                await _context.DonHangs
                    .Include(d =>
                        d.ChiTietDonHangs)
                    .Where(d =>
                        d.MaNguoiDung ==
                        user.MaNguoiDung)
                    .AsNoTracking()
                    .OrderByDescending(
                        d => d.NgayDat)
                    .ToListAsync();


            var model =
                orders.Select(
                    d => new DonHangViewModel
                    {
                        MaDonHang =
                            d.MaDonHang,

                        NgayDat =
                            d.NgayDat,

                        TongTien =
                            d.TongTien,

                        TrangThai =
                            d.TrangThai,

                        PhuongThucThanhToan =
                            d.PhuongThucThanhToan,

                        DiaChiGiaoHang =
                            d.DiaChiGiaoHang,

                        GhiChu =
                            d.GhiChu,

                        MaNguoiDung =
                            d.MaNguoiDung,

                        TenKhachHang =
                            user.HoTen,

                        EmailKhachHang =
                            user.Email,

                        TongSoLuong =
                            d.ChiTietDonHangs
                                .Sum(x => x.SoLuong)
                    })
                    .ToList();


            return View(model);
        }


        // =========================================================
        // USER - CHI TIẾT ĐƠN HÀNG
        // =========================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChiTietCuaToi(
            int id)
        {
            var user =
                await GetCurrentUser();


            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var donHang =
                await _context.DonHangs
                    .Include(d =>
                        d.NguoiDung)
                    .Include(d =>
                        d.ChiTietDonHangs)
                        .ThenInclude(ct =>
                            ct.Sach)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        d =>
                            d.MaDonHang == id &&
                            d.MaNguoiDung ==
                            user.MaNguoiDung);


            if (donHang == null)
            {
                return NotFound();
            }


            var model =
                new DonHangChiTietViewModel
                {
                    MaDonHang =
                        donHang.MaDonHang,

                    NgayDat =
                        donHang.NgayDat,

                    TongTien =
                        donHang.TongTien,

                    TrangThai =
                        donHang.TrangThai,

                    PhuongThucThanhToan =
                        donHang.PhuongThucThanhToan,

                    DiaChiGiaoHang =
                        donHang.DiaChiGiaoHang,

                    GhiChu =
                        donHang.GhiChu,

                    TenKhachHang =
                        donHang.NguoiDung?.HoTen
                        ?? user.HoTen,

                    EmailKhachHang =
                        donHang.NguoiDung?.Email
                        ?? user.Email
                };


            foreach (
                var ct in
                donHang.ChiTietDonHangs)
            {
                model.SanPhams.Add(
                    new DonHangSanPhamViewModel
                    {
                        MaSach =
                            ct.MaSach,

                        TenSach =
                            ct.Sach?.TenSach
                            ?? "Không xác định",

                        Hinh =
                            ct.Sach?.Hinh,

                        SoLuong =
                            ct.SoLuong,

                        DonGia =
                            ct.DonGia,

                        ThanhTien =
                            ct.ThanhTien
                    });
            }


            return View(
                "ChiTietCuaToi",
                model);
        }


        // =========================================================
        // ADMIN - THỐNG KÊ
        // =========================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ThongKe()
        {
            var orders =
                await _context.DonHangs
                    .AsNoTracking()
                    .ToListAsync();


            // -----------------------------------------------------
            // CHỈ ĐẾM ĐƠN ĐÃ XÁC NHẬN / ĐÃ THANH TOÁN
            // LÀ DOANH THU
            // -----------------------------------------------------

            var donDaChot =
                orders.Where(
                    d =>
                        d.TrangThai ==
                        "Đã xác nhận"
                        ||
                        d.TrangThai ==
                        "Đã thanh toán");


            var model =
                new ThongKeDonHangViewModel
                {
                    TongSoDon =
                        orders.Count,

                    SoDonDaXacNhan =
                        orders.Count(
                            d =>
                                d.TrangThai ==
                                "Đã xác nhận"
                                ||
                                d.TrangThai ==
                                "Đã thanh toán"),

                    SoDonChoXacNhan =
                        orders.Count(
                            d =>
                                d.TrangThai ==
                                "Chờ Admin xác nhận"),

                    SoDonDaHuy =
                        orders.Count(
                            d =>
                                d.TrangThai ==
                                "Đã hủy"),

                    TongDoanhThu =
                        donDaChot
                            .Sum(d => d.TongTien),

                    DoanhThuVNPAY =
                        donDaChot
                            .Where(d =>
                                d.PhuongThucThanhToan ==
                                "VNPAY")
                            .Sum(d => d.TongTien),

                    DoanhThuCOD =
                        donDaChot
                            .Where(d =>
                                d.PhuongThucThanhToan ==
                                "COD")
                            .Sum(d => d.TongTien),

                    DoanhThuChuyenKhoan =
                        donDaChot
                            .Where(d =>
                                d.PhuongThucThanhToan ==
                                "BANK")
                            .Sum(d => d.TongTien)
                };


            return View(model);
        }
    }
}