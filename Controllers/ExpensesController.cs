using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AdminPanel.Controllers
{
    // Kun Admin må bruge denne controller
    [Authorize(Roles = "Admin")]
    public class ExpensesController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // Henter og viser alle udgifter
        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses.ToListAsync();
            return View(expenses);
        }

        // Viser formular til oprettelse af udgift
        // GET: Expenses/Create
        public IActionResult Create()
        {
            return View();
        }

        // Opretter ny udgift
        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Product,Date,Quantity,PricePerPiece,Notes")] Expense expense)
        {
            // Tjekker om model er valid
            if (ModelState.IsValid)
            {
                // Beregner totalpris
                // antal * pris pr stk
                expense.TotalPrice = expense.Quantity * expense.PricePerPiece;

                // Gemmer udgift i database
                _context.Add(expense);

                // Gemmer ændringer i SQL database
                await _context.SaveChangesAsync();

                // Sender bruger tilbage til Index siden
                return RedirectToAction(nameof(Index));
            }

            // Returnerer view igen hvis model ikke er valid
            return View(expense);
        }

        // Henter udgift og viser edit side
        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Hvis id mangler
            if (id == null)
                return NotFound();

            // Finder udgift i database
            var expense = await _context.Expenses.FindAsync(id);

            // Hvis udgift ikke findes
            if (expense == null)
                return NotFound();

            // Sender udgift til view
            return View(expense);
        }

        // Opdaterer eksisterende udgift
        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Product,Date,Quantity,PricePerPiece,Notes")] Expense expense)
        {
            // Finder eksisterende udgift i database
            var expenseToUpdate = await _context.Expenses.FindAsync(id);

            // Hvis udgift ikke findes
            if (expenseToUpdate == null)
                return NotFound();

            // Tjekker om model er valid
            if (ModelState.IsValid)
            {
                // Opdaterer felter
                expenseToUpdate.Product = expense.Product;
                expenseToUpdate.Date = expense.Date;
                expenseToUpdate.Quantity = expense.Quantity;
                expenseToUpdate.PricePerPiece = expense.PricePerPiece;
                expenseToUpdate.Notes = expense.Notes;

                // Beregner ny totalpris
                expenseToUpdate.TotalPrice =
                    expenseToUpdate.Quantity * expenseToUpdate.PricePerPiece;

                // Gemmer ændringer i database
                await _context.SaveChangesAsync();

                // Sender bruger tilbage til Index siden
                return RedirectToAction(nameof(Index));
            }

            // Returnerer view igen hvis model ikke er valid
            return View(expenseToUpdate);
        }

        // Henter udgift som skal slettes
        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Hvis id mangler
            if (id == null)
                return NotFound();

            // Finder udgift i database
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id);

            // Hvis udgift ikke findes
            if (expense == null)
                return NotFound();

            // Sender udgift til delete view
            return View(expense);
        }

        // Sletter udgift fra database
        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Finder udgift i database
            var expense = await _context.Expenses.FindAsync(id);

            // Hvis udgift findes
            if (expense != null)
            {
                // Fjerner udgift fra database
                _context.Expenses.Remove(expense);

                // Gemmer ændringer
                await _context.SaveChangesAsync();
            }

            // Sender bruger tilbage til Index siden
            return RedirectToAction(nameof(Index));
        }
    }
}