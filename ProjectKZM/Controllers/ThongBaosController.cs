using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Services;
using System.Security.Claims;

namespace ProjectKZM.Controllers
{
    [Authorize]
    public class ThongBaosController : Controller
    {
        private readonly MyDbContext _context;
        private readonly ThongBaoService _thongBaoService;

        public ThongBaosController(
            MyDbContext context,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _thongBaoService = thongBaoService;
        }


        // =====================================================
        // XEM TẤT CẢ
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email =
                User.FindFirstValue(
                    ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var user =
                await _context.NguoiDungs
                    .FirstOrDefaultAsync(
                        x => x.Email == email);

            if (user == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var notifications =
                await _context.ThongBaos
                    .Where(x =>
                        x.MaNguoiDung ==
                        user.MaNguoiDung)
                    .OrderByDescending(
                        x => x.NgayTao)
                    .Select(x =>
                        new ViewModels.ThongBaoViewModel
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

            return View(notifications);
        }


        // =====================================================
        // ĐÁNH DẤU ĐÃ ĐỌC
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DaDoc(
            int id)
        {
            var email =
                User.FindFirstValue(
                    ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var user =
                await _context.NguoiDungs
                    .FirstOrDefaultAsync(
                        x => x.Email == email);

            if (user == null)
            {
                return Unauthorized();
            }

            var notification =
                await _context.ThongBaos
                    .FirstOrDefaultAsync(
                        x =>
                            x.MaThongBao == id &&
                            x.MaNguoiDung ==
                                user.MaNguoiDung);

            if (notification == null)
            {
                return NotFound();
            }

            notification.DaDoc =
                true;

            await _context.SaveChangesAsync();


            if (!string.IsNullOrWhiteSpace(
                notification.DuongDan))
            {
                return Redirect(
                    notification.DuongDan);
            }

            return RedirectToAction(
                nameof(Index));
        }


        // =====================================================
        // ĐÁNH DẤU TẤT CẢ
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DaDocTatCa()
        {
            var email =
                User.FindFirstValue(
                    ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var user =
                await _context.NguoiDungs
                    .FirstOrDefaultAsync(
                        x => x.Email == email);

            if (user == null)
            {
                return Unauthorized();
            }

            var notifications =
                await _context.ThongBaos
                    .Where(x =>
                        x.MaNguoiDung ==
                            user.MaNguoiDung &&
                        !x.DaDoc)
                    .ToListAsync();

            foreach (var notification
                     in notifications)
            {
                notification.DaDoc =
                    true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }
    }
}