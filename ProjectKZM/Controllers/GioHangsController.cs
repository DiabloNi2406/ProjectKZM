using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class GioHangsController : Controller
    {
        private readonly MyDbContext _context;

        public GioHangsController(MyDbContext context)
        {
            _context = context;
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
        // LẤY GIỎ HÀNG CỦA USER
        // =========================================================

        private async Task<GioHang?> GetCurrentCart()
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return null;
            }

            return await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);
        }


        // =========================================================
        // INDEX - XEM GIỎ HÀNG
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);

            // Chưa có giỏ hàng
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaNguoiDung = user.MaNguoiDung,
                    ChiTietGioHangs = new List<ChiTietGioHang>()
                };
            }

            return View(gioHang);
        }


        // =========================================================
        // THÊM VÀO GIỎ HÀNG
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(
            int maSach,
            int soLuong = 1)
        {
            if (soLuong <= 0)
            {
                soLuong = 1;
            }

            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            // =====================================================
            // KIỂM TRA SÁCH
            // =====================================================

            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s => s.MaSach == maSach);

            if (sach == null)
            {
                return NotFound();
            }


            // =====================================================
            // KIỂM TRA TỒN KHO
            // =====================================================

            if (sach.SoLuong <= 0)
            {
                TempData["Error"] =
                    "Sản phẩm đã hết hàng.";

                return RedirectToAction("Index", "Home");
            }


            // =====================================================
            // TÌM GIỎ HÀNG
            // =====================================================

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);


            // =====================================================
            // NẾU CHƯA CÓ GIỎ HÀNG -> TẠO
            // =====================================================

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    MaNguoiDung = user.MaNguoiDung,
                    NgayTao = DateTime.Now
                };

                _context.GioHangs.Add(gioHang);

                await _context.SaveChangesAsync();
            }


            // =====================================================
            // KIỂM TRA SÁCH ĐÃ CÓ TRONG GIỎ CHƯA
            // =====================================================

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct =>
                    ct.MaGioHang == gioHang.MaGioHang &&
                    ct.MaSach == maSach);


            // =====================================================
            // CHƯA CÓ -> TẠO CHI TIẾT MỚI
            // =====================================================

            if (chiTiet == null)
            {
                chiTiet = new ChiTietGioHang
                {
                    MaGioHang = gioHang.MaGioHang,
                    MaSach = maSach,
                    SoLuong = Math.Min(
                        soLuong,
                        sach.SoLuong)
                };

                _context.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                // =================================================
                // ĐÃ CÓ -> TĂNG SỐ LƯỢNG
                // =================================================

                chiTiet.SoLuong += soLuong;


                // Không vượt quá tồn kho
                if (chiTiet.SoLuong > sach.SoLuong)
                {
                    chiTiet.SoLuong = sach.SoLuong;
                }
            }


            // =====================================================
            // LƯU DATABASE
            // =====================================================

            await _context.SaveChangesAsync();


            TempData["Success"] =
                $"Đã thêm \"{sach.TenSach}\" vào giỏ hàng.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // TĂNG SỐ LƯỢNG
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tang(
            int maGioHang,
            int maSach)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g =>
                    g.MaGioHang == maGioHang &&
                    g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null)
            {
                return NotFound();
            }


            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct =>
                    ct.MaGioHang == maGioHang &&
                    ct.MaSach == maSach);

            if (chiTiet == null)
            {
                return NotFound();
            }


            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s =>
                    s.MaSach == maSach);

            if (sach == null)
            {
                return NotFound();
            }


            if (chiTiet.SoLuong < sach.SoLuong)
            {
                chiTiet.SoLuong++;

                await _context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // GIẢM SỐ LƯỢNG
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Giam(
            int maGioHang,
            int maSach)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g =>
                    g.MaGioHang == maGioHang &&
                    g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null)
            {
                return NotFound();
            }


            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct =>
                    ct.MaGioHang == maGioHang &&
                    ct.MaSach == maSach);

            if (chiTiet == null)
            {
                return NotFound();
            }


            if (chiTiet.SoLuong > 1)
            {
                chiTiet.SoLuong--;
            }
            else
            {
                // Khi giảm từ 1 xuống 0 thì sản phẩm bị xóa
                _context.ChiTietGioHangs.Remove(chiTiet);

                TempData["Success"] =
                    "Đã xóa sản phẩm khỏi danh sách giỏ hàng.";
            }


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // THANH TOÁN SẢN PHẨM ĐÃ CHỌN
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(
    int[] maSach,
    int[] soLuong)
        {
            // =====================================================
            // KIỂM TRA DỮ LIỆU
            // =====================================================

            if (maSach == null ||
                soLuong == null ||
                maSach.Length == 0 ||
                soLuong.Length == 0 ||
                maSach.Length != soLuong.Length)
            {
                TempData["Error"] =
                    "Vui lòng chọn sản phẩm để thanh toán.";

                return RedirectToAction(nameof(Index));
            }


            // =====================================================
            // USER
            // =====================================================

            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // =====================================================
            // GIỎ HÀNG
            // =====================================================

            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);


            if (gioHang == null)
            {
                TempData["Error"] =
                    "Không tìm thấy giỏ hàng.";

                return RedirectToAction(nameof(Index));
            }


            // =====================================================
            // KIỂM TRA SẢN PHẨM ĐƯỢC CHỌN
            // =====================================================

            for (int i = 0; i < maSach.Length; i++)
            {
                int idSach = maSach[i];
                int qty = soLuong[i];


                if (qty <= 0)
                {
                    TempData["Error"] =
                        "Số lượng thanh toán không hợp lệ.";

                    return RedirectToAction(nameof(Index));
                }


                var chiTiet =
                    gioHang.ChiTietGioHangs
                        .FirstOrDefault(
                            ct => ct.MaSach == idSach);


                if (chiTiet == null ||
                    chiTiet.Sach == null)
                {
                    TempData["Error"] =
                        "Có sản phẩm không tồn tại trong giỏ hàng.";

                    return RedirectToAction(nameof(Index));
                }


                if (qty > chiTiet.SoLuong)
                {
                    TempData["Error"] =
                        $"Số lượng thanh toán của " +
                        $"\"{chiTiet.Sach.TenSach}\" " +
                        "vượt quá số lượng trong giỏ.";

                    return RedirectToAction(nameof(Index));
                }


                if (qty > chiTiet.Sach.SoLuong)
                {
                    TempData["Error"] =
                        $"Sách \"{chiTiet.Sach.TenSach}\" " +
                        "không đủ số lượng trong kho.";

                    return RedirectToAction(nameof(Index));
                }
            }


            // =====================================================
            // CHUYỂN SANG THANHTOANCONTROLLER.INDEX
            // =====================================================

            return RedirectToAction(
                "Index",
                "ThanhToan",
                new
                {
                    maSach = maSach,
                    soLuong = soLuong
                });
        }


        // =========================================================
        // XÓA MỘT SẢN PHẨM
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(
            int maGioHang,
            int maSach)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            // =====================================================
            // KIỂM TRA GIỎ HÀNG CÓ THUỘC USER KHÔNG
            // =====================================================

            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(g =>
                    g.MaGioHang == maGioHang &&
                    g.MaNguoiDung == user.MaNguoiDung);

            if (gioHang == null)
            {
                return NotFound();
            }


            // =====================================================
            // TÌM SẢN PHẨM TRONG GIỎ
            // =====================================================

            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct =>
                    ct.MaGioHang == maGioHang &&
                    ct.MaSach == maSach);


            // =====================================================
            // XÓA SẢN PHẨM
            // =====================================================

            if (chiTiet != null)
            {
                _context.ChiTietGioHangs.Remove(chiTiet);

                await _context.SaveChangesAsync();


                // =================================================
                // THÔNG BÁO XÓA THÀNH CÔNG
                // =================================================

                TempData["Success"] =
                    "Đã xóa sản phẩm khỏi danh sách giỏ hàng.";
            }
            else
            {
                TempData["Error"] =
                    "Không tìm thấy sản phẩm trong giỏ hàng.";
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // XÓA TOÀN BỘ GIỎ HÀNG
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCa()
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }


            var gioHang = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .FirstOrDefaultAsync(
                    g => g.MaNguoiDung == user.MaNguoiDung);


            if (gioHang != null &&
                gioHang.ChiTietGioHangs.Any())
            {
                _context.ChiTietGioHangs.RemoveRange(
                    gioHang.ChiTietGioHangs);

                await _context.SaveChangesAsync();


                TempData["Success"] =
                    "Đã xóa toàn bộ sản phẩm khỏi danh sách giỏ hàng.";
            }
            else
            {
                TempData["Error"] =
                    "Giỏ hàng không có sản phẩm để xóa.";
            }


            return RedirectToAction(nameof(Index));
        }
    }

}