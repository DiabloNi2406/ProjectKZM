using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class YeuThichsController : Controller
    {
        private readonly MyDbContext _context;

        public YeuThichsController(MyDbContext context)
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
                .FirstOrDefaultAsync(x =>
                    x.Email == email);
        }


        // =========================================================
        // DANH SÁCH YÊU THÍCH
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var danhSachYeuThich =
                await _context.YeuThichs
                    .Include(x => x.Sach)
                        .ThenInclude(s => s!.LoaiSach)

                    .Include(x => x.Sach)
                        .ThenInclude(s => s!.SachTheLoais)
                            .ThenInclude(st => st.TheLoai)

                    .Where(x =>
                        x.MaNguoiDung ==
                        user.MaNguoiDung)

                    .OrderByDescending(x => x.NgayThem)

                    .ToListAsync();


            return View(danhSachYeuThich);
        }


        // =========================================================
        // THÊM YÊU THÍCH
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(int maSach)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return Unauthorized();
            }


            var sach =
                await _context.Sachs
                    .FirstOrDefaultAsync(x =>
                        x.MaSach == maSach);


            if (sach == null)
            {
                return NotFound();
            }


            // =====================================================
            // KIỂM TRA ĐÃ CÓ CHƯA
            // =====================================================

            var daTonTai =
                await _context.YeuThichs
                    .AnyAsync(x =>
                        x.MaNguoiDung ==
                        user.MaNguoiDung
                        &&
                        x.MaSach ==
                        maSach);


            // =====================================================
            // CHƯA CÓ -> THÊM
            // =====================================================

            if (!daTonTai)
            {
                var yeuThich =
                    new YeuThich
                    {
                        MaNguoiDung =
                            user.MaNguoiDung,

                        MaSach =
                            maSach,

                        NgayThem =
                            DateTime.Now
                    };


                _context.YeuThichs.Add(
                    yeuThich);

                await _context.SaveChangesAsync();
            }


            // =====================================================
            // NẾU GỌI BẰNG JAVASCRIPT FETCH
            // =====================================================

            if (Request.Headers["X-Requested-With"]
                == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    active = true
                });
            }


            // =====================================================
            // GỌI BẰNG FORM THÔNG THƯỜNG
            // =====================================================

            TempData["Success"] =
                $"Đã thêm \"{sach.TenSach}\" vào danh sách yêu thích.";


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================================
        // XÓA KHỎI YÊU THÍCH
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int maSach)
        {
            var user = await GetCurrentUser();

            if (user == null)
            {
                return Unauthorized();
            }


            var yeuThich =
                await _context.YeuThichs
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiDung ==
                        user.MaNguoiDung
                        &&
                        x.MaSach ==
                        maSach);


            if (yeuThich != null)
            {
                _context.YeuThichs.Remove(
                    yeuThich);

                await _context.SaveChangesAsync();
            }


            // =====================================================
            // NẾU GỌI BẰNG JAVASCRIPT FETCH
            // =====================================================

            if (Request.Headers["X-Requested-With"]
                == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    active = false
                });
            }


            // =====================================================
            // GỌI BẰNG FORM THÔNG THƯỜNG
            // =====================================================

            TempData["Success"] =
                "Đã xóa sản phẩm khỏi danh sách yêu thích.";


            return RedirectToAction(
                nameof(Index));
        }
    }
}