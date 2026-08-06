
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Models;
using ProjectKZM.Data;

public class NguoiDungsController : Controller
{
    private readonly MyDbContext _context;

    public NguoiDungsController(MyDbContext context)
    {
        _context = context;
    }

    // GET: NGUOIDUNGS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.NguoiDungs.ToListAsync());
    }

    // GET: NGUOIDUNGS/Details/5
    public async Task<IActionResult> Details(int? manguoidung)
    {
        if (manguoidung == null)
        {
            return NotFound();
        }

        var nguoidung = await _context.NguoiDungs
            .FirstOrDefaultAsync(m => m.MaNguoiDung == manguoidung);
        if (nguoidung == null)
        {
            return NotFound();
        }

        return View(nguoidung);
    }

    // GET: NGUOIDUNGS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: NGUOIDUNGS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaNguoiDung,HoTen,Email,MatKhau,SoDienThoai,DiaChi,AnhDaiDien,MaVaiTro,VaiTro,GioHangs,DonHangs")] NguoiDung nguoidung)
    {
        if (ModelState.IsValid)
        {
            _context.Add(nguoidung);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(nguoidung);
    }

    // GET: NGUOIDUNGS/Edit/5
    public async Task<IActionResult> Edit(int? manguoidung)
    {
        if (manguoidung == null)
        {
            return NotFound();
        }

        var nguoidung = await _context.NguoiDungs.FindAsync(manguoidung);
        if (nguoidung == null)
        {
            return NotFound();
        }
        return View(nguoidung);
    }

    // POST: NGUOIDUNGS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? manguoidung, [Bind("MaNguoiDung,HoTen,Email,MatKhau,SoDienThoai,DiaChi,AnhDaiDien,MaVaiTro,VaiTro,GioHangs,DonHangs")] NguoiDung nguoidung)
    {
        if (manguoidung != nguoidung.MaNguoiDung)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(nguoidung);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NguoiDungExists(nguoidung.MaNguoiDung))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(nguoidung);
    }

    // GET: NGUOIDUNGS/Delete/5
    public async Task<IActionResult> Delete(int? manguoidung)
    {
        if (manguoidung == null)
        {
            return NotFound();
        }

        var nguoidung = await _context.NguoiDungs
            .FirstOrDefaultAsync(m => m.MaNguoiDung == manguoidung);
        if (nguoidung == null)
        {
            return NotFound();
        }

        return View(nguoidung);
    }

    // POST: NGUOIDUNGS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? manguoidung)
    {
        var nguoidung = await _context.NguoiDungs.FindAsync(manguoidung);
        if (nguoidung != null)
        {
            _context.NguoiDungs.Remove(nguoidung);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool NguoiDungExists(int? manguoidung)
    {
        return _context.NguoiDungs.Any(e => e.MaNguoiDung == manguoidung);
    }
}
