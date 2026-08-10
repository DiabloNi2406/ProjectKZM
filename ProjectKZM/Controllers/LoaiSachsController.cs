using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Data;
using ProjectKZM.Models;

namespace ProjectKZM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoaiSachsController : Controller
    {
        private readonly MyDbContext _context;

        public LoaiSachsController(MyDbContext context)
        {
            _context = context;
        }


        // ==========================================
        // INDEX
        // ==========================================

        public async Task<IActionResult> Index()
        {
            var loaiSachs = await _context.LoaiSachs
                .Include(x => x.Sachs)
                .OrderBy(x => x.TenLoaiSach)
                .ToListAsync();

            return View(loaiSachs);
        }


        // ==========================================
        // DETAILS
        // ==========================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiSach = await _context.LoaiSachs
                .Include(x => x.Sachs)
                .FirstOrDefaultAsync(x => x.MaLoaiSach == id);

            if (loaiSach == null)
            {
                return NotFound();
            }

            return View(loaiSach);
        }


        // ==========================================
        // CREATE - GET
        // ==========================================

        public IActionResult Create()
        {
            return View();
        }


        // ==========================================
        // CREATE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LoaiSach loaiSach)
        {
            if (ModelState.IsValid)
            {
                _context.Add(loaiSach);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(loaiSach);
        }


        // ==========================================
        // EDIT - GET
        // ==========================================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiSach = await _context.LoaiSachs
                .FindAsync(id);

            if (loaiSach == null)
            {
                return NotFound();
            }

            return View(loaiSach);
        }


        // ==========================================
        // EDIT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            LoaiSach loaiSach)
        {
            if (id != loaiSach.MaLoaiSach)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(loaiSach);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LoaiSachExists(loaiSach.MaLoaiSach))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(loaiSach);
        }


        // ==========================================
        // DELETE - GET
        // ==========================================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var loaiSach = await _context.LoaiSachs
                .Include(x => x.Sachs)
                .FirstOrDefaultAsync(x => x.MaLoaiSach == id);

            if (loaiSach == null)
            {
                return NotFound();
            }

            return View(loaiSach);
        }


        // ==========================================
        // DELETE - POST
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var loaiSach = await _context.LoaiSachs
                .FindAsync(id);

            if (loaiSach != null)
            {
                _context.LoaiSachs.Remove(loaiSach);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        private bool LoaiSachExists(int id)
        {
            return _context.LoaiSachs
                .Any(e => e.MaLoaiSach == id);
        }
    }
}