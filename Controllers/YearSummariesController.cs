using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    [Authorize(Roles = "Admin")]
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
                .Where(o => o.SetupDate.HasValue)
                .Select(o => o.SetupDate.Value.Year)
                .Distinct()
                .ToListAsync();

            var expenseYears = await _context.Expenses
                .Select(e => e.Date.Year)
                .Distinct()
                .ToListAsync();

            var years = orderYears.Union(expenseYears).OrderByDescending(y => y);

            var summaries = new List<YearSummary>();

            foreach (var year in years)
            {
                // Hent orders for året
                var ordersForYear = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.SetupDate.HasValue && o.SetupDate.Value.Year == year)
                    .AsEnumerable(); // Client-side beregning

                decimal omsætning = ordersForYear.Sum(o => o.Price + (o.Transport ?? 0));
                decimal udgifter = _context.Expenses
                    .Where(e => e.Date.Year == year)
                    .Sum(e => e.TotalPrice);

                int antalJobs = ordersForYear.Count();

                summaries.Add(new YearSummary
                {
                    Year = year,
                    Revenue = omsætning,
                    Expenses = udgifter,
                    NumberOfJobs = antalJobs,
                    YearSummaries = omsætning - udgifter
                });
            }

            return View(summaries);
        }
    }
}