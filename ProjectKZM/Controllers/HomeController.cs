using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;

namespace ProjectKZM.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;

        public HomeController(MyDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // HOME
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int[]? theLoaiIds,
            int[]? loaiSachIds)
        {
            // =====================================================
            // LẤY DANH SÁCH SÁCH
            // =====================================================

            IQueryable<Sach> query = _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.SachTheLoais)
                    .ThenInclude(st => st.TheLoai)
                .AsQueryable();


            // =====================================================
            // KIỂM TRA CÓ ĐANG SEARCH KHÔNG
            // =====================================================

            bool hasSearch =
                Request.Query.ContainsKey("search");


            // =====================================================
            // KIỂM TRA CÓ ĐANG LỌC KHÔNG
            // =====================================================

            bool hasFilter =
                Request.Query.ContainsKey("theLoaiIds") ||
                Request.Query.ContainsKey("loaiSachIds");


            // =====================================================
            // ĐANG SEARCH HOẶC LỌC
            // =====================================================

            bool isSearchingOrFiltering =
                hasSearch || hasFilter;


            // =====================================================
            // TÌM KIẾM
            // =====================================================

            if (hasSearch)
            {
                search = search?.Trim();

                // Người dùng bấm Search nhưng không nhập gì
                if (string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s => false);
                }
                else
                {
                    query = query.Where(s =>
                        s.TenSach.Contains(search));
                }
            }


            // =====================================================
            // LỌC THEO LOẠI SÁCH
            // =====================================================

            if (loaiSachIds != null &&
                loaiSachIds.Length > 0)
            {
                query = query.Where(s =>
                    s.MaLoaiSach.HasValue &&
                    loaiSachIds.Contains(s.MaLoaiSach.Value));
            }


            // =====================================================
            // LỌC THEO THỂ LOẠI
            // =====================================================

            if (theLoaiIds != null &&
                theLoaiIds.Length > 0)
            {
                query = query.Where(s =>
                    s.SachTheLoais.Any(st =>
                        theLoaiIds.Contains(st.MaTheLoai)));
            }


            // =====================================================
            // LẤY SÁCH
            // =====================================================

            var sachs = await query
                .OrderBy(s => s.TenSach)
                .ToListAsync();


            // =====================================================
            // LẤY 6 SÁCH MỚI
            // =====================================================

            var sachMoi = await _context.Sachs
                .Include(s => s.LoaiSach)
                .Include(s => s.SachTheLoais)
                    .ThenInclude(st => st.TheLoai)
                .OrderByDescending(s => s.MaSach)
                .Take(6)
                .ToListAsync();


            // =====================================================
            // LẤY THỂ LOẠI CHO FILTER
            // =====================================================

            var theLoais = await _context.TheLoais
                .OrderBy(x => x.TenTheLoai)
                .ToListAsync();


            // =====================================================
            // LẤY LOẠI SÁCH CHO FILTER
            // =====================================================

            var loaiSachs = await _context.LoaiSachs
                .OrderBy(x => x.TenLoaiSach)
                .ToListAsync();


            // =====================================================
            // VIEWBAG
            // =====================================================

            ViewBag.TheLoais = theLoais;

            ViewBag.LoaiSachs = loaiSachs;

            ViewBag.SelectedTheLoaiIds =
                theLoaiIds ?? Array.Empty<int>();

            ViewBag.SelectedLoaiSachIds =
                loaiSachIds ?? Array.Empty<int>();

            ViewBag.Search =
                search ?? "";

            ViewBag.SachMoi =
                sachMoi;


            // =====================================================
            // GỬI TRẠNG THÁI SEARCH / FILTER SANG INDEX
            // =====================================================

            ViewBag.IsSearchingOrFiltering =
                isSearchingOrFiltering;


            // =====================================================
            // TRẢ VỀ INDEX
            // =====================================================

            return View(sachs);
        }
    }
}