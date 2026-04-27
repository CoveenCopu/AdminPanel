using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses.ToListAsync();
            return View(expenses);
        }

        // GET: Expenses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Produkt,Dato,Antal,PrisPrStk,Noter")] Expense expense)
        {
            if (ModelState.IsValid)
            {
                // Beregn TotalPris
                expense.TotalPris = expense.Antal * expense.PrisPrStk;

                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Produkt,Dato,Antal,PrisPrStk,Noter")] Expense expense)
        {
            var expenseToUpdate = await _context.Expenses.FindAsync(id);
            if (expenseToUpdate == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Opdater felter
                expenseToUpdate.Produkt = expense.Produkt;
                expenseToUpdate.Dato = expense.Dato;
                expenseToUpdate.Antal = expense.Antal;
                expenseToUpdate.PrisPrStk = expense.PrisPrStk;
                expenseToUpdate.Noter = expense.Noter;

                // Beregn TotalPris igen
                expenseToUpdate.TotalPris = expenseToUpdate.Antal * expenseToUpdate.PrisPrStk;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(expenseToUpdate);
        }

        // GET: Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExpenseExists(int id)
        {
            return _context.Expenses.Any(e => e.Id == id);
        }
    }
}