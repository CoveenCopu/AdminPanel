using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class ProdukterController : Controller
    {
        private readonly AppDbContext _context;

        public ProdukterController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Produkter.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produkt produkt)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produkt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produkt);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var produkt = await _context.Produkter.FirstOrDefaultAsync(p => p.Id == id);
            if (produkt == null) return NotFound();

            return View(produkt);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produkt = await _context.Produkter.FindAsync(id);
            if (produkt == null) return NotFound();

            return View(produkt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Produkt produkt)
        {
            if (id != produkt.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(produkt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(produkt);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produkt = await _context.Produkter.FirstOrDefaultAsync(p => p.Id == id);
            if (produkt == null) return NotFound();

            return View(produkt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produkt = await _context.Produkter.FindAsync(id);
            _context.Produkter.Remove(produkt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}