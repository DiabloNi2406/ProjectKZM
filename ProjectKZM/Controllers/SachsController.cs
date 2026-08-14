
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    public class SachsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SachsController(
            MyDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var sachs = await _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.SachTheLoais)
                    .ThenInclude(st => st.TheLoai)
                .ToListAsync();

            return View(sachs);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            // =====================================================
            // LẤY SÁCH ĐANG XEM
            // =====================================================

            var sach = await _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.SachTheLoais)
                    .ThenInclude(st => st.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }


            // =====================================================
            // LẤY ID CÁC THỂ LOẠI CỦA SÁCH ĐANG XEM
            // =====================================================

            var theLoaiIds = sach.SachTheLoais?
                .Select(x => x.MaTheLoai)
                .Distinct()
                .ToList()
                ?? new List<int>();


            // =====================================================
            // SÁCH CÙNG TÁC GIẢ
            // =====================================================

            var sachCungTacGia = new List<Sach>();

            if (!string.IsNullOrWhiteSpace(sach.TacGia))
            {
                var tacGia = sach.TacGia.Trim();

                var cungTacGiaCandidates = await _context.Sachs
                    .AsNoTracking()
                    .Include(s => s.LoaiSach)
                    .Include(s => s.SachTheLoais)
                        .ThenInclude(st => st.TheLoai)
                    .Where(s =>
                        s.MaSach != sach.MaSach &&
                        s.TacGia != null &&
                        s.TacGia.Trim() == tacGia)
                    .ToListAsync();

                // -------------------------------------------------
                // Trộn ngẫu nhiên
                // -------------------------------------------------

                sachCungTacGia = cungTacGiaCandidates
                    .OrderBy(x => Random.Shared.Next())
                    .Take(10)
                    .ToList();
            }


            // =====================================================
            // ID SÁCH ĐÃ CÓ Ở HÀNG CÙNG TÁC GIẢ
            // =====================================================

            var sachCungTacGiaIds = sachCungTacGia
                .Select(x => x.MaSach)
                .ToHashSet();


            // =====================================================
            // NỘI DUNG ĐỀ XUẤT
            //
            // Cùng ít nhất một thể loại với sách hiện tại
            // Không lấy sách hiện tại
            // Không trùng với hàng "Cùng tác giả"
            // Tối đa 10 cuốn
            // =====================================================

            var sachDeXuat = new List<Sach>();

            if (theLoaiIds.Any())
            {
                var deXuatCandidates = await _context.Sachs
                    .AsNoTracking()
                    .Include(s => s.LoaiSach)
                    .Include(s => s.SachTheLoais)
                        .ThenInclude(st => st.TheLoai)
                    .Where(s =>
                        s.MaSach != sach.MaSach &&
                        !sachCungTacGiaIds.Contains(s.MaSach) &&
                        s.SachTheLoais.Any(
                            st => theLoaiIds.Contains(st.MaTheLoai)
                        ))
                    .ToListAsync();

                // -------------------------------------------------
                // Trộn ngẫu nhiên
                // -------------------------------------------------

                sachDeXuat = deXuatCandidates
                    .OrderBy(x => Random.Shared.Next())
                    .Take(10)
                    .ToList();
            }


            // =====================================================
            // LẤY DANH SÁCH SÁCH ĐÃ YÊU THÍCH
            //
            // GIỐNG HOME CONTROLLER
            // ĐỌC TRỰC TIẾP TỪ DATABASE
            // =====================================================

            var favoriteBookIds = new List<int>();

            if (User.Identity?.IsAuthenticated == true)
            {
                var email =
                    User.FindFirstValue(ClaimTypes.Email);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    var user =
                        await _context.NguoiDungs
                            .FirstOrDefaultAsync(x =>
                                x.Email == email);

                    if (user != null)
                    {
                        favoriteBookIds =
                            await _context.YeuThichs
                                .Where(x =>
                                    x.MaNguoiDung ==
                                    user.MaNguoiDung)
                                .Select(x => x.MaSach)
                                .ToListAsync();
                    }
                }
            }


            // =====================================================
            // GỬI 2 DANH SÁCH SANG VIEW
            // =====================================================

            ViewBag.SachCungTacGia =
                sachCungTacGia;

            ViewBag.SachDeXuat =
                sachDeXuat;


            // =====================================================
            // GỬI DANH SÁCH YÊU THÍCH SANG DETAILS
            //
            // ĐỂ F5 VẪN GIỮ TRẠNG THÁI
            // =====================================================

            ViewBag.FavoriteBookIds =
                favoriteBookIds.ToArray();


            // =====================================================
            // TRẢ VIEW
            // =====================================================

            return View(sach);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        public async Task<IActionResult> Create()
        {
            await LoadFormData();

            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Sach sach,
            int[] selectedTheLoaiIds,
            IFormFile? HinhFile)
        {
            // -----------------------------------------------------
            // Kiểm tra Loại sách
            // -----------------------------------------------------

            if (sach.MaLoaiSach <= 0)
            {
                ModelState.AddModelError(
                    "MaLoaiSach",
                    "Vui lòng chọn loại sách."
                );
            }


            // -----------------------------------------------------
            // Kiểm tra Thể loại
            // -----------------------------------------------------

            if (selectedTheLoaiIds == null ||
                selectedTheLoaiIds.Length == 0)
            {
                ModelState.AddModelError(
                    "SachTheLoais",
                    "Vui lòng chọn ít nhất một thể loại."
                );
            }


            // -----------------------------------------------------
            // Kiểm tra hình ảnh
            // -----------------------------------------------------

            if (HinhFile != null && HinhFile.Length > 0)
            {
                var extension =
                    Path.GetExtension(HinhFile.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "Hinh",
                        "Chỉ chấp nhận JPG, JPEG, PNG hoặc WEBP."
                    );
                }

                if (HinhFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Hinh",
                        "Kích thước hình ảnh không được vượt quá 5MB."
                    );
                }
            }


            // -----------------------------------------------------
            // Nếu hợp lệ
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                // =================================================
                // UPLOAD HÌNH
                // =================================================

                if (HinhFile != null && HinhFile.Length > 0)
                {
                    sach.Hinh = await SaveImage(HinhFile);
                }


                // =================================================
                // THÊM SÁCH
                // =================================================

                _context.Sachs.Add(sach);

                await _context.SaveChangesAsync();


                // =================================================
                // THÊM NHIỀU THỂ LOẠI
                // =================================================

                var distinctTheLoaiIds =
                    (selectedTheLoaiIds ?? Array.Empty<int>())
                    .Distinct()
                    .ToList();

                foreach (var maTheLoai in distinctTheLoaiIds)
                {
                    _context.SachTheLoais.Add(
                        new SachTheLoai
                        {
                            MaSach = sach.MaSach,
                            MaTheLoai = maTheLoai
                        }
                    );
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // ModelState lỗi → load lại dữ liệu
            // -----------------------------------------------------

            await LoadFormData(
                sach.MaLoaiSach,
                selectedTheLoaiIds
            );

            return View(sach);
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .Include(s => s.SachTheLoais)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }


            // =====================================================
            // LẤY DANH SÁCH LOẠI SÁCH
            // =====================================================

            ViewBag.MaLoaiSach = new SelectList(
                await _context.LoaiSachs
                    .OrderBy(x => x.TenLoaiSach)
                    .ToListAsync(),
                "MaLoaiSach",
                "TenLoaiSach",
                sach.MaLoaiSach
            );


            // =====================================================
            // LẤY DANH SÁCH THỂ LOẠI
            // =====================================================

            ViewBag.TheLoais = await _context.TheLoais
                .OrderBy(x => x.TenTheLoai)
                .ToListAsync();


            // =====================================================
            // CÁC THỂ LOẠI SÁCH HIỆN TẠI
            // =====================================================

            ViewBag.SelectedTheLoaiIds =
                sach.SachTheLoais
                    .Select(x => x.MaTheLoai)
                    .ToList();


            return View(sach);
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Sach sach,
            int[] selectedTheLoaiIds,
            IFormFile? HinhFile)
        {
            // -----------------------------------------------------
            // Kiểm tra ID
            // -----------------------------------------------------

            if (id != sach.MaSach)
            {
                return NotFound();
            }


            // -----------------------------------------------------
            // Kiểm tra Loại sách
            // -----------------------------------------------------

            if (sach.MaLoaiSach <= 0)
            {
                ModelState.AddModelError(
                    "MaLoaiSach",
                    "Vui lòng chọn loại sách."
                );
            }


            // -----------------------------------------------------
            // Kiểm tra Thể loại
            // -----------------------------------------------------

            if (selectedTheLoaiIds == null ||
                selectedTheLoaiIds.Length == 0)
            {
                ModelState.AddModelError(
                    "SachTheLoais",
                    "Vui lòng chọn ít nhất một thể loại."
                );
            }


            // -----------------------------------------------------
            // Kiểm tra hình mới
            // -----------------------------------------------------

            if (HinhFile != null && HinhFile.Length > 0)
            {
                var extension =
                    Path.GetExtension(HinhFile.FileName)
                    .ToLowerInvariant();

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "Hinh",
                        "Chỉ chấp nhận JPG, JPEG, PNG hoặc WEBP."
                    );
                }

                if (HinhFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "Hinh",
                        "Kích thước hình ảnh không được vượt quá 5MB."
                    );
                }
            }


            // -----------------------------------------------------
            // Nếu ModelState hợp lệ
            // -----------------------------------------------------

            if (ModelState.IsValid)
            {
                var sachDb = await _context.Sachs
                    .Include(s => s.SachTheLoais)
                    .FirstOrDefaultAsync(s => s.MaSach == id);

                if (sachDb == null)
                {
                    return NotFound();
                }


                // =================================================
                // CẬP NHẬT THÔNG TIN SÁCH
                // =================================================

                sachDb.TenSach = sach.TenSach;
                sachDb.Chuong = sach.Chuong;
                sachDb.TacGia = sach.TacGia;
                sachDb.NhaXuatBan = sach.NhaXuatBan;
                sachDb.Gia = sach.Gia;
                sachDb.SoLuong = sach.SoLuong;
                sachDb.MoTa = sach.MoTa;
                sachDb.MaLoaiSach = sach.MaLoaiSach;


                // =================================================
                // CẬP NHẬT HÌNH
                // =================================================

                if (HinhFile != null && HinhFile.Length > 0)
                {
                    // Xóa hình cũ
                    DeleteImage(sachDb.Hinh);

                    // Lưu hình mới
                    sachDb.Hinh = await SaveImage(HinhFile);
                }


                // =================================================
                // XÓA TOÀN BỘ THỂ LOẠI CŨ
                // =================================================

                _context.SachTheLoais.RemoveRange(
                    sachDb.SachTheLoais
                );


                // =================================================
                // THÊM LẠI THỂ LOẠI MỚI
                // =================================================

                var distinctTheLoaiIds =
                    (selectedTheLoaiIds ?? Array.Empty<int>())
                    .Distinct()
                    .ToList();

                foreach (var maTheLoai in distinctTheLoaiIds)
                {
                    _context.SachTheLoais.Add(
                        new SachTheLoai
                        {
                            MaSach = sachDb.MaSach,
                            MaTheLoai = maTheLoai
                        }
                    );
                }


                // =================================================
                // LƯU
                // =================================================

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            // -----------------------------------------------------
            // ModelState lỗi → load lại dữ liệu
            // -----------------------------------------------------

            await LoadFormData(
                sach.MaLoaiSach,
                selectedTheLoaiIds
            );

            return View(sach);
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.SachTheLoais)
                    .ThenInclude(st => st.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            return View(sach);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Sachs
                .Include(s => s.SachTheLoais)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach != null)
            {
                // =================================================
                // XÓA CÁC QUAN HỆ THỂ LOẠI
                // =================================================

                _context.SachTheLoais.RemoveRange(
                    sach.SachTheLoais
                );


                // =================================================
                // XÓA HÌNH
                // =================================================

                DeleteImage(sach.Hinh);


                // =================================================
                // XÓA SÁCH
                // =================================================

                _context.Sachs.Remove(sach);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // LOAD FORM DATA
        // =========================================================

        private async Task LoadFormData(
            int? selectedLoaiSachId = null,
            int[]? selectedTheLoaiIds = null)
        {
            // -----------------------------------------------------
            // Loại sách
            // -----------------------------------------------------

            ViewBag.MaLoaiSach = new SelectList(
                await _context.LoaiSachs
                    .OrderBy(x => x.TenLoaiSach)
                    .ToListAsync(),
                "MaLoaiSach",
                "TenLoaiSach",
                selectedLoaiSachId
            );


            // -----------------------------------------------------
            // Thể loại A-Z
            // -----------------------------------------------------

            ViewBag.TheLoais = await _context.TheLoais
                .OrderBy(x => x.TenTheLoai)
                .ToListAsync();


            // -----------------------------------------------------
            // Danh sách ID đã chọn
            // -----------------------------------------------------

            ViewBag.SelectedTheLoaiIds =
                selectedTheLoaiIds?
                    .Distinct()
                    .ToList()
                ?? new List<int>();
        }


        // =========================================================
        // SAVE IMAGE
        // =========================================================

        private async Task<string> SaveImage(IFormFile file)
        {
            string uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "sach"
                );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }


            string extension =
                Path.GetExtension(file.FileName);


            string fileName =
                Guid.NewGuid().ToString()
                + extension;


            string filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName
                );


            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            return "/images/sach/" + fileName;
        }


        // =========================================================
        // DELETE IMAGE
        // =========================================================

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return;
            }

            try
            {
                string fileName =
                    Path.GetFileName(imagePath);


                string filePath =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "sach",
                        fileName
                    );


                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Không làm crash chương trình
                // nếu xóa hình thất bại
            }
        }


        // =========================================================
        // CHECK EXISTS
        // =========================================================

        private bool SachExists(int id)
        {
            return _context.Sachs
                .Any(e => e.MaSach == id);
        }
    }
}