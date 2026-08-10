using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;

namespace ProjectKZM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TheLoaisController : Controller
    {
        private readonly MyDbContext _context;

        public TheLoaisController(MyDbContext context)
        {
            _context = context;
        }


        // ==========================================================
        // INDEX
        // ==========================================================

        public async Task<IActionResult> Index()
        {
            var theLoais = await _context.TheLoais
                .Include(t => t.SachTheLoais)
                .ThenInclude(st => st.Sach)
                .ToListAsync();

            return View(theLoais);
        }


        // ==========================================================
        // DETAILS
        // ==========================================================

        public async Task<IActionResult> Details(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoais
                .Include(t => t.SachTheLoais)
                .ThenInclude(st => st.Sach)
                .FirstOrDefaultAsync(
                    t => t.MaTheLoai == matheloai);

            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }


        // ==========================================================
        // CREATE - GET
        // ==========================================================

        public IActionResult Create()
        {
            return View();
        }


        // ==========================================================
        // CREATE - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TheLoai theLoai)
        {
            if (ModelState.IsValid)
            {
                _context.TheLoais.Add(theLoai);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(theLoai);
        }


        // ==========================================================
        // EDIT - GET
        // ==========================================================

        public async Task<IActionResult> Edit(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoais
                .FindAsync(matheloai);

            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }


        // ==========================================================
        // EDIT - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int matheloai,
            [Bind("MaTheLoai,TenTheLoai,MoTa,Hinh")]
            TheLoai theLoai)
        {
            if (matheloai != theLoai.MaTheLoai)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(theLoai);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TheLoaiExists(theLoai.MaTheLoai))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(theLoai);
        }


        // ==========================================================
        // DELETE - GET
        // ==========================================================

        public async Task<IActionResult> Delete(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theLoai = await _context.TheLoais
                .Include(t => t.SachTheLoais)
                .ThenInclude(st => st.Sach)
                .FirstOrDefaultAsync(
                    t => t.MaTheLoai == matheloai);

            if (theLoai == null)
            {
                return NotFound();
            }

            return View(theLoai);
        }


        // ==========================================================
        // DELETE - POST
        // ==========================================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int matheloai)
        {
            var theLoai = await _context.TheLoais
                .FindAsync(matheloai);

            if (theLoai != null)
            {
                _context.TheLoais.Remove(theLoai);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ==========================================================
        // CHECK EXISTS
        // ==========================================================

        private bool TheLoaiExists(int matheloai)
        {
            return _context.TheLoais
                .Any(e => e.MaTheLoai == matheloai);
        }
    }
}