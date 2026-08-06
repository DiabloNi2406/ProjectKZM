
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Models;
using ProjectKZM.Data;

public class TheLoaisController : Controller
{
    private readonly MyDbContext _context;

    public TheLoaisController(MyDbContext context)
    {
        _context = context;
    }

    // GET: THELOAIS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.TheLoais.ToListAsync());
    }

    // GET: THELOAIS/Details/5
    public async Task<IActionResult> Details(int? matheloai)
    {
        if (matheloai == null)
        {
            return NotFound();
        }

        var theloai = await _context.TheLoais
            .FirstOrDefaultAsync(m => m.MaTheLoai == matheloai);
        if (theloai == null)
        {
            return NotFound();
        }

        return View(theloai);
    }

    // GET: THELOAIS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: THELOAIS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaTheLoai,TenTheLoai,MoTa,Hinh,Sachs")] TheLoai theloai)
    {
        if (ModelState.IsValid)
        {
            _context.Add(theloai);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(theloai);
    }

    // GET: THELOAIS/Edit/5
    public async Task<IActionResult> Edit(int? matheloai)
    {
        if (matheloai == null)
        {
            return NotFound();
        }

        var theloai = await _context.TheLoais.FindAsync(matheloai);
        if (theloai == null)
        {
            return NotFound();
        }
        return View(theloai);
    }

    // POST: THELOAIS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? matheloai, [Bind("MaTheLoai,TenTheLoai,MoTa,Hinh,Sachs")] TheLoai theloai)
    {
        if (matheloai != theloai.MaTheLoai)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(theloai);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TheLoaiExists(theloai.MaTheLoai))
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
        return View(theloai);
    }

    // GET: THELOAIS/Delete/5
    public async Task<IActionResult> Delete(int? matheloai)
    {
        if (matheloai == null)
        {
            return NotFound();
        }

        var theloai = await _context.TheLoais
            .FirstOrDefaultAsync(m => m.MaTheLoai == matheloai);
        if (theloai == null)
        {
            return NotFound();
        }

        return View(theloai);
    }

    // POST: THELOAIS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? matheloai)
    {
        var theloai = await _context.TheLoais.FindAsync(matheloai);
        if (theloai != null)
        {
            _context.TheLoais.Remove(theloai);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TheLoaiExists(int? matheloai)
    {
        return _context.TheLoais.Any(e => e.MaTheLoai == matheloai);
    }
}
