using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers
{
    // Kun admins må bruge Products controller
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // Henter og viser alle produkter
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // Viser formular til oprettelse af produkt
        public IActionResult Create()
        {
            return View();
        }

        // Opretter nyt produkt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Tjekker om model er valid
            if (ModelState.IsValid)
            {
                // Tilføjer produkt til database
                _context.Add(product);

                // Gemmer ændringer i SQL database
                await _context.SaveChangesAsync();

                // Sender bruger tilbage til Index siden
                return RedirectToAction(nameof(Index));
            }

            // Returnerer view igen hvis model ikke er valid
            return View(product);
        }

        // Viser detaljer om et produkt
        public async Task<IActionResult> Details(int? id)
        {
            // Hvis id mangler
            if (id == null)
                return NotFound();

            // Finder produkt i database
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            // Hvis produkt ikke findes
            if (product == null)
                return NotFound();

            // Sender produkt til view
            return View(product);
        }

        // Henter produkt til redigering
        public async Task<IActionResult> Edit(int? id)
        {
            // Hvis id mangler
            if (id == null)
                return NotFound();

            // Finder produkt i database
            var product = await _context.Products.FindAsync(id);

            // Hvis produkt ikke findes
            if (product == null)
                return NotFound();

            // Sender produkt til view
            return View(product);
        }

        // Opdaterer eksisterende produkt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmed(int id, Product product)
        {
            // Tjekker om route-id matcher produkt-id
            if (id != product.Id)
                return NotFound();

            // Tjekker om model er valid
            if (ModelState.IsValid)
            {
                // Opdaterer produkt i database
                _context.Update(product);

                // Gemmer ændringer
                await _context.SaveChangesAsync();

                // Sender bruger tilbage til Index siden
                return RedirectToAction(nameof(Index));
            }

            // Returnerer view igen hvis model ikke er valid
            return View(product);
        }

        // Henter produkt som skal slettes
        public async Task<IActionResult> Delete(int? id)
        {
            // Hvis id mangler
            if (id == null)
                return NotFound();

            // Finder produkt i database
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            // Hvis produkt ikke findes
            if (product == null)
                return NotFound();

            // Sender produkt til delete view
            return View(product);
        }

        // Sletter produkt fra database
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Finder produkt i database
            var product = await _context.Products.FindAsync(id);

            // Hvis produkt ikke findes
            if (product == null)
                return NotFound();

            // Fjerner produkt fra database
            _context.Products.Remove(product);

            // Gemmer ændringer
            await _context.SaveChangesAsync();

            // Sender bruger tilbage til Index siden
            return RedirectToAction(nameof(Index));
        }
    }
}