using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;
using ProjectKZM.ViewModels;
using System.Security.Claims;

namespace ProjectKZM.Services
{
    public class ThongBaoService
    {
        private readonly MyDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThongBaoService(
            MyDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        // =====================================================
        // USER HIỆN TẠI
        // =====================================================

        private async Task<NguoiDung?> GetCurrentUserAsync()
        {
            var userPrincipal =
                _httpContextAccessor.HttpContext?.User;

            var email =
                userPrincipal?.FindFirstValue(
                    ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            return await _context.NguoiDungs
                .FirstOrDefaultAsync(
                    x => x.Email == email);
        }


        // =====================================================
        // LẤY THÔNG BÁO CHO USER HIỆN TẠI
        // =====================================================

        public async Task<List<ThongBaoViewModel>>
            GetNotificationsAsync(
                int maxCount = 20)
        {
            var user =
                await GetCurrentUserAsync();

            if (user == null)
            {
                return new List<ThongBaoViewModel>();
            }

            return await _context.ThongBaos
                .Where(x =>
                    x.MaNguoiDung ==
                    user.MaNguoiDung)
                .OrderByDescending(
                    x => x.NgayTao)
                .Take(maxCount)
                .Select(x =>
                    new ThongBaoViewModel
                    {
                        MaThongBao =
                            x.MaThongBao,

                        TieuDe =
                            x.TieuDe,

                        NoiDung =
                            x.NoiDung,

                        DuongDan =
                            x.DuongDan,

                        Loai =
                            x.Loai,

                        MaDonHang =
                            x.MaDonHang,

                        DaDoc =
                            x.DaDoc,

                        NgayTao =
                            x.NgayTao
                    })
                .ToListAsync();
        }


        // =====================================================
        // ĐẾM CHƯA ĐỌC
        // =====================================================

        public async Task<int>
            GetUnreadCountAsync()
        {
            var user =
                await GetCurrentUserAsync();

            if (user == null)
            {
                return 0;
            }

            return await _context.ThongBaos
                .CountAsync(
                    x =>
                        x.MaNguoiDung ==
                        user.MaNguoiDung &&
                        !x.DaDoc);
        }


        // =====================================================
        // TẠO THÔNG BÁO CHO 1 USER
        // =====================================================

        public async Task CreateAsync(
            int maNguoiDung,
            string tieuDe,
            string noiDung,
            string? duongDan = null,
            string? loai = null,
            int? maDonHang = null)
        {
            var thongBao =
                new ThongBao
                {
                    MaNguoiDung =
                        maNguoiDung,

                    TieuDe =
                        tieuDe,

                    NoiDung =
                        noiDung,

                    DuongDan =
                        duongDan,

                    Loai =
                        loai,

                    MaDonHang =
                        maDonHang,

                    DaDoc =
                        false,

                    NgayTao =
                        DateTime.Now
                };

            _context.ThongBaos.Add(
                thongBao);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // TẠO THÔNG BÁO CHO TẤT CẢ ADMIN
        // =====================================================

        public async Task CreateForAdminsAsync(
            string tieuDe,
            string noiDung,
            string? duongDan = null,
            string? loai = null,
            int? maDonHang = null)
        {
            var adminUsers =
                await _context.NguoiDungs
                    .Include(x => x.VaiTro)
                    .Where(x =>
                        x.VaiTro != null &&
                        x.VaiTro.TenVaiTro == "Admin")
                    .ToListAsync();

            foreach (var admin in adminUsers)
            {
                _context.ThongBaos.Add(
                    new ThongBao
                    {
                        MaNguoiDung =
                            admin.MaNguoiDung,

                        TieuDe =
                            tieuDe,

                        NoiDung =
                            noiDung,

                        DuongDan =
                            duongDan,

                        Loai =
                            loai,

                        MaDonHang =
                            maDonHang,

                        DaDoc =
                            false,

                        NgayTao =
                            DateTime.Now
                    });
            }

            await _context.SaveChangesAsync();
        }
    }
}