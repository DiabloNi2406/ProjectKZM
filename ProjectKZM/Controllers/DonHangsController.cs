using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.Services;
using ProjectKZM.ViewModels;
using System.Security.Claims;


namespace ProjectKZM.Controllers
{
    [Authorize]
    public class DonHangsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ThongBaoService _thongBaoService;

        public DonHangsController(
        MyDbContext context,
        ThongBaoService thongBaoService)
        {
            _context = context;
            _thongBaoService = thongBaoService;
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

                    LyDoHuy =
                        donHang.LyDoHuy,

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

                await _thongBaoService.CreateAsync(
                donHang.MaNguoiDung,
                "Đơn hàng đã được xác nhận",
                $"Đơn hàng #{donHang.MaDonHang} của bạn đã được Admin xác nhận.",
                $"/DonHangs/ChiTietCuaToi/{donHang.MaDonHang}",
                "XAC_NHAN",
                donHang.MaDonHang);

                await transaction.CommitAsync();


                TempData["Success"] =
                    $"Đã xác nhận đơn hàng #{id}.";


                return RedirectToAction(
                    nameof(ChiTiet),
                    new { id });
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
            int id,
            string lyDoHuy)
        {
            // ---------------------------------------------------------
            // BẮT BUỘC NHẬP LÝ DO HỦY
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(lyDoHuy))
            {
                TempData["Error"] =
                    "Vui lòng nhập lý do hủy đơn.";

                return RedirectToAction(
                    nameof(ChiTiet),
                    new { id });
            }


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


                // -----------------------------------------------------
                // CHỈ HỦY ĐƠN PENDING
                // -----------------------------------------------------

                if (donHang.TrangThai !=
                    "Chờ Admin xác nhận")
                {
                    TempData["Error"] =
                        "Chỉ có thể hủy đơn đang chờ xác nhận.";

                    return RedirectToAction(
                        nameof(ChoXacNhan));
                }


                // -----------------------------------------------------
                // TRẢ LẠI HÀNG ĐÃ GIỮ
                // -----------------------------------------------------

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


                // -----------------------------------------------------
                // LƯU LÝ DO HỦY + ĐÁNH DẤU HỦY
                // -----------------------------------------------------

                donHang.LyDoHuy =
                    lyDoHuy.Trim();

                donHang.TrangThai =
                    "Đã hủy";


                // -----------------------------------------------------
                // LƯU DATABASE
                // -----------------------------------------------------

                await _context.SaveChangesAsync();


                // -----------------------------------------------------
                // THÔNG BÁO CHO USER
                // -----------------------------------------------------

                await _thongBaoService.CreateAsync(
                donHang.MaNguoiDung,
                "Đơn hàng đã bị hủy",
                $"Đơn hàng #{donHang.MaDonHang} đã bị hủy. Lý do: {donHang.LyDoHuy}",
                $"/DonHangs/ChiTietCuaToi/{donHang.MaDonHang}",
                "HUY",
                donHang.MaDonHang);



                // -----------------------------------------------------
                // COMMIT TRANSACTION
                // -----------------------------------------------------

                await transaction.CommitAsync();


                TempData["Success"] =
                    $"Đã hủy đơn hàng #{id} và trả hàng về kho.";


                return RedirectToAction(
                    nameof(ChiTiet),
                    new { id });
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

                    LyDoHuy =
                        donHang.LyDoHuy,

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

        // =====================================================
        // DỮ LIỆU BIỂU ĐỒ DOANH THU
        // =====================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DoanhThuChart(
            string period = "month")
        {
            period =
                string.IsNullOrWhiteSpace(period)
                    ? "month"
                    : period.ToLowerInvariant();


            DateTime now =
                DateTime.Now;


            DateTime from;
            DateTime to;


            // =================================================
            // TUẦN
            // =================================================

            if (period == "week")
            {
                int diff =
                    (7 +
                     (now.DayOfWeek -
                      DayOfWeek.Monday))
                    % 7;


                from =
                    now.Date.AddDays(-diff);


                to =
                    from.AddDays(7);
            }


            // =================================================
            // NĂM
            // =================================================

            else if (period == "year")
            {
                from =
                    new DateTime(
                        now.Year,
                        1,
                        1);


                to =
                    from.AddYears(1);
            }


            // =================================================
            // THÁNG
            // =================================================

            else
            {
                period = "month";


                from =
                    new DateTime(
                        now.Year,
                        now.Month,
                        1);


                to =
                    from.AddMonths(1);
            }


            // =================================================
            // LẤY ĐƠN HÀNG
            //
            // GIỐNG HOÀN TOÀN LOGIC ThongKe()
            // =================================================

            var orders =
                await _context.DonHangs
                    .AsNoTracking()
                    .Where(d =>
                        d.NgayDat >= from &&
                        d.NgayDat < to &&
                        (
                            d.TrangThai ==
                                "Đã xác nhận"

                            ||

                            d.TrangThai ==
                                "Đã thanh toán"
                        ))
                    .Select(d => new
                    {
                        d.NgayDat,

                        d.TongTien,

                        d.PhuongThucThanhToan,

                        d.TrangThai
                    })
                    .ToListAsync();


            // =================================================
            // DANH SÁCH TRẢ VỀ
            // =================================================

            var labels =
                new List<string>();


            var vnpay =
                new List<decimal>();


            var cod =
                new List<decimal>();


            var bank =
                new List<decimal>();


            // =================================================
            // HÀM XỬ LÝ 1 NGÀY
            // =================================================

            void AddDay(
                DateTime date)
            {
                labels.Add(
                    date.ToString(
                        "dd/MM"));


                var dayOrders =
                    orders.Where(
                        d =>
                            d.NgayDat.Date ==
                            date.Date);


                // ---------------------------------------------
                // VNPAY
                // ---------------------------------------------

                vnpay.Add(
                    dayOrders
                        .Where(
                            d =>
                                d.PhuongThucThanhToan
                                == "VNPAY")
                        .Sum(
                            d => d.TongTien));


                // ---------------------------------------------
                // COD
                // ---------------------------------------------

                cod.Add(
                    dayOrders
                        .Where(
                            d =>
                                d.PhuongThucThanhToan
                                == "COD")
                        .Sum(
                            d => d.TongTien));


                // ---------------------------------------------
                // BANK
                // ---------------------------------------------

                bank.Add(
                    dayOrders
                        .Where(
                            d =>
                                d.PhuongThucThanhToan
                                == "BANK")
                        .Sum(
                            d => d.TongTien));
            }


            // =================================================
            // TUẦN
            // =================================================

            if (period == "week")
            {
                for (int i = 0;
                     i < 7;
                     i++)
                {
                    AddDay(
                        from.AddDays(i));
                }
            }


            // =================================================
            // THÁNG
            // =================================================

            else if (period == "month")
            {
                int days =
                    DateTime.DaysInMonth(
                        now.Year,
                        now.Month);


                for (int day = 1;
                     day <= days;
                     day++)
                {
                    AddDay(
                        new DateTime(
                            now.Year,
                            now.Month,
                            day));
                }
            }


            // =================================================
            // NĂM
            // =================================================

            else
            {
                for (int month = 1;
                     month <= 12;
                     month++)
                {
                    labels.Add(
                        $"T{month}");


                    var monthOrders =
                        orders.Where(
                            d =>
                                d.NgayDat.Year
                                    == now.Year
                                &&
                                d.NgayDat.Month
                                    == month);


                    // -----------------------------------------
                    // VNPAY
                    // -----------------------------------------

                    vnpay.Add(
                        monthOrders
                            .Where(
                                d =>
                                    d.PhuongThucThanhToan
                                    == "VNPAY")
                            .Sum(
                                d => d.TongTien));


                    // -----------------------------------------
                    // COD
                    // -----------------------------------------

                    cod.Add(
                        monthOrders
                            .Where(
                                d =>
                                    d.PhuongThucThanhToan
                                    == "COD")
                            .Sum(
                                d => d.TongTien));


                    // -----------------------------------------
                    // BANK
                    // -----------------------------------------

                    bank.Add(
                        monthOrders
                            .Where(
                                d =>
                                    d.PhuongThucThanhToan
                                    == "BANK")
                            .Sum(
                                d => d.TongTien));
                }
            }


            // =================================================
            // DEBUG
            // =================================================

            Console.WriteLine(
                $"[CHART] Period = {period}");

            Console.WriteLine(
                $"[CHART] Orders = {orders.Count}");

            Console.WriteLine(
                $"[CHART] VNPAY = {vnpay.Sum()}");

            Console.WriteLine(
                $"[CHART] COD = {cod.Sum()}");

            Console.WriteLine(
                $"[CHART] BANK = {bank.Sum()}");


            // =================================================
            // JSON
            // =================================================

            return Json(
                new
                {
                    period,

                    labels,

                    vnpay,

                    cod,

                    bank
                });
        }
    }
}