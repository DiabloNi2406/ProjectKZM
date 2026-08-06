
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectKZM.Models;
using ProjectKZM.Data;

public class VaiTrosController : Controller
{
    private readonly MyDbContext _context;

    public VaiTrosController(MyDbContext context)
    {
        _context = context;
    }

    // GET: VAITROS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.VaiTros.ToListAsync());
    }

    // GET: VAITROS/Details/5
    public async Task<IActionResult> Details(int? mavaitro)
    {
        if (mavaitro == null)
        {
            return NotFound();
        }

        var vaitro = await _context.VaiTros
            .FirstOrDefaultAsync(m => m.MaVaiTro == mavaitro);
        if (vaitro == null)
        {
            return NotFound();
        }

        return View(vaitro);
    }

    // GET: VAITROS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: VAITROS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaVaiTro,TenVaiTro,NguoiDungs")] VaiTro vaitro)
    {
        if (ModelState.IsValid)
        {
            _context.Add(vaitro);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(vaitro);
    }

    // GET: VAITROS/Edit/5
    public async Task<IActionResult> Edit(int? mavaitro)
    {
        if (mavaitro == null)
        {
            return NotFound();
        }

        var vaitro = await _context.VaiTros.FindAsync(mavaitro);
        if (vaitro == null)
        {
            return NotFound();
        }
        return View(vaitro);
    }

    // POST: VAITROS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? mavaitro, [Bind("MaVaiTro,TenVaiTro,NguoiDungs")] VaiTro vaitro)
    {
        if (mavaitro != vaitro.MaVaiTro)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(vaitro);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VaiTroExists(vaitro.MaVaiTro))
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
        return View(vaitro);
    }

    // GET: VAITROS/Delete/5
    public async Task<IActionResult> Delete(int? mavaitro)
    {
        if (mavaitro == null)
        {
            return NotFound();
        }

        var vaitro = await _context.VaiTros
            .FirstOrDefaultAsync(m => m.MaVaiTro == mavaitro);
        if (vaitro == null)
        {
            return NotFound();
        }

        return View(vaitro);
    }

    // POST: VAITROS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? mavaitro)
    {
        var vaitro = await _context.VaiTros.FindAsync(mavaitro);
        if (vaitro != null)
        {
            _context.VaiTros.Remove(vaitro);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool VaiTroExists(int? mavaitro)
    {
        return _context.VaiTros.Any(e => e.MaVaiTro == mavaitro);
    }
}
