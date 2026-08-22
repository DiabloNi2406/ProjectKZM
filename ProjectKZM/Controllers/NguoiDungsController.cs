using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NguoiDungsController : Controller
    {
        private readonly MyDbContext _context;

        public NguoiDungsController(MyDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // INDEX
        // =====================================================

        public async Task<IActionResult> Index()
        {
            var nguoiDungs = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .ToListAsync();

            return View(nguoiDungs);
        }

        // =====================================================
        // DETAILS
        // =====================================================

        public async Task<IActionResult> Details(int? manguoidung)
        {
            if (manguoidung == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.MaNguoiDung == manguoidung);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }

        // =====================================================
        // CREATE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadRolesAsync();

            return View(new NguoiDung());
        }

        // =====================================================
        // CREATE - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("HoTen,Email,MatKhau,SoDienThoai,DiaChi,AnhDaiDien,MaVaiTro")]
            NguoiDung nguoiDung)
        {
            // ---------------------------------------------
            // Kiểm tra Email
            // ---------------------------------------------

            if (!string.IsNullOrWhiteSpace(nguoiDung.Email))
            {
                bool emailExists = await _context.NguoiDungs
                    .AnyAsync(x => x.Email == nguoiDung.Email);

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Email này đã tồn tại.");
                }
            }

            // ---------------------------------------------
            // Kiểm tra Role
            // ---------------------------------------------

            var roleExists = await _context.VaiTros
                .AnyAsync(x => x.MaVaiTro == nguoiDung.MaVaiTro);

            if (!roleExists)
            {
                ModelState.AddModelError(
                    "MaVaiTro",
                    "Vui lòng chọn vai trò.");
            }

            // ---------------------------------------------
            // Nếu lỗi
            // ---------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadRolesAsync(nguoiDung.MaVaiTro);

                return View(nguoiDung);
            }

            // ---------------------------------------------
            // Ảnh mặc định nếu chưa chọn ảnh
            // ---------------------------------------------

            if (string.IsNullOrWhiteSpace(nguoiDung.AnhDaiDien))
            {
                nguoiDung.AnhDaiDien = "/images/Icon/user.jpg";
            }

            // ---------------------------------------------
            // Thêm người dùng
            // ---------------------------------------------

            _context.NguoiDungs.Add(nguoiDung);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Thêm người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // EDIT - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int? manguoidung)
        {
            if (manguoidung == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(x =>
                    x.MaNguoiDung == manguoidung);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            await LoadRolesAsync(nguoiDung.MaVaiTro);

            return View(nguoiDung);
        }


        // =====================================================
        // EDIT - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            NguoiDung nguoiDung,
            IFormFile? anhDaiDienFile)
        {
            // -----------------------------------------------------
            // Lấy đúng người dùng từ MaNguoiDung gửi từ form
            // -----------------------------------------------------

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x =>
                    x.MaNguoiDung == nguoiDung.MaNguoiDung);

            if (user == null)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Kiểm tra Model
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadRolesAsync(user.MaVaiTro);

                return View(nguoiDung);
            }


            // -----------------------------------------------------
            // CẬP NHẬT THÔNG TIN
            // -----------------------------------------------------

            user.HoTen = nguoiDung.HoTen;
            user.Email = nguoiDung.Email;
            user.MatKhau = nguoiDung.MatKhau;
            user.SoDienThoai = nguoiDung.SoDienThoai;
            user.DiaChi = nguoiDung.DiaChi;


            // -----------------------------------------------------
            // GIỮ NGUYÊN VAI TRÒ HIỆN TẠI
            // -----------------------------------------------------

            user.MaVaiTro = user.MaVaiTro;


            // -----------------------------------------------------
            // CẬP NHẬT AVATAR
            // -----------------------------------------------------

            if (anhDaiDienFile != null &&
                anhDaiDienFile.Length > 0)
            {
                string uploadsFolder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "avatars");


                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }


                string extension =
                    Path.GetExtension(
                        anhDaiDienFile.FileName);


                string newFileName =
                    Guid.NewGuid().ToString()
                    + extension;


                string filePath =
                    Path.Combine(
                        uploadsFolder,
                        newFileName);


                // -------------------------------------------------
                // LƯU ẢNH MỚI
                // -------------------------------------------------

                using (var stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    await anhDaiDienFile.CopyToAsync(stream);
                }


                // -------------------------------------------------
                // XÓA ẢNH CŨ
                // -------------------------------------------------

                if (!string.IsNullOrEmpty(user.AnhDaiDien))
                {
                    string oldFileName =
                        Path.GetFileName(user.AnhDaiDien);

                    string oldFilePath =
                        Path.Combine(
                            uploadsFolder,
                            oldFileName);


                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }


                // -------------------------------------------------
                // LƯU TÊN ẢNH MỚI VÀO DATABASE
                // -------------------------------------------------

                user.AnhDaiDien =
                    newFileName;
            }


            // -----------------------------------------------------
            // LƯU DATABASE
            // -----------------------------------------------------

            await _context.SaveChangesAsync();


            // -----------------------------------------------------
            // QUAY VỀ DANH SÁCH NGƯỜI DÙNG
            // -----------------------------------------------------

            TempData["Success"] =
                "Cập nhật người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // DELETE - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int? manguoidung)
        {
            if (manguoidung == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x =>
                    x.MaNguoiDung == manguoidung);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Không cho Admin tự xóa mình

            string? currentEmail =
                User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(currentEmail) &&
                nguoiDung.Email == currentEmail)
            {
                ViewBag.Error =
                    "Admin không thể tự xóa bản thân!";

                return View(nguoiDung);
            }

            return View(nguoiDung);
        }

        // =====================================================
        // DELETE - POST
        // =====================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int manguoidung)
        {
            var nguoiDung = await _context.NguoiDungs
                .FindAsync(manguoidung);

            if (nguoiDung == null)
            {
                return NotFound();
            }

            // Không cho Admin tự xóa mình

            string? currentEmail =
                User.FindFirstValue(ClaimTypes.Email);

            if (!string.IsNullOrEmpty(currentEmail) &&
                nguoiDung.Email == currentEmail)
            {
                TempData["Error"] =
                    "Admin không thể tự xóa bản thân!";

                return RedirectToAction(nameof(Index));
            }

            _context.NguoiDungs.Remove(nguoiDung);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Xóa người dùng thành công!";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // LOAD ROLE
        // =====================================================

        private async Task LoadRolesAsync(int? selectedRole = null)
        {
            var roles = await _context.VaiTros
                .OrderBy(x => x.TenVaiTro)
                .ToListAsync();

            ViewBag.Roles = new SelectList(
                roles,
                "MaVaiTro",
                "TenVaiTro",
                selectedRole
            );
        }

        // =====================================================
        // EXISTS
        // =====================================================

        private bool NguoiDungExists(int id)
        {
            return _context.NguoiDungs
                .Any(x => x.MaNguoiDung == id);
        }
    }
}