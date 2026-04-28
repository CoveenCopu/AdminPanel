using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    [Authorize]
    public class YearSummariesController : Controller
    {
        private readonly AppDbContext _context;

        public YearSummariesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Find alle år med Orders eller Expenses
            var orderYears = await _context.Orders
                .Where(o => o.Opsætningsdato.HasValue)
                .Select(o => o.Opsætningsdato.Value.Year)
                .Distinct()
                .ToListAsync();

            var expenseYears = await _context.Expenses
                .Select(e => e.Dato.Year)
                .Distinct()
                .ToListAsync();

            var years = orderYears.Union(expenseYears).OrderByDescending(y => y);

            var summaries = new List<YearSummary>();

            foreach (var year in years)
            {
                // Hent orders for året
                var ordersForYear = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Produkt)
                    .Where(o => o.Opsætningsdato.HasValue && o.Opsætningsdato.Value.Year == year)
                    .AsEnumerable(); // Client-side beregning

                decimal omsætning = ordersForYear.Sum(o => o.Pris + (o.Transport ?? 0));
                decimal udgifter = _context.Expenses
                    .Where(e => e.Dato.Year == year)
                    .Sum(e => e.TotalPris);

                int antalJobs = ordersForYear.Count();

                summaries.Add(new YearSummary
                {
                    År = year,
                    Omsætning = omsætning,
                    Udgifter = udgifter,
                    AntalJobs = antalJobs,
                    Årsopgørelse = omsætning - udgifter
                });
            }

            return View(summaries);
        }
    }
}