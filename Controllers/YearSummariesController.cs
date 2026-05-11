using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AdminPanel.Controllers
{
    // Kun admins må bruge YearSummaries controller
    [Authorize(Roles = "Admin")]
    public class YearSummariesController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public YearSummariesController(AppDbContext context)
        {
            _context = context;
        }

        // Henter og viser årsopgørelser
        public async Task<IActionResult> Index()
        {
            // Finder alle år hvor der findes ordrer
            var orderYears = await _context.Orders
                .Where(o => o.SetupDate.HasValue)
                .Select(o => o.SetupDate.Value.Year)
                .Distinct()
                .ToListAsync();

            // Finder alle år hvor der findes udgifter
            var expenseYears = await _context.Expenses
                .Select(e => e.Date.Year)
                .Distinct()
                .ToListAsync();

            // Samler alle år og sorterer dem faldende
            var years = orderYears
                .Union(expenseYears)
                .OrderByDescending(y => y);

            // Liste som skal indeholde årsopgørelser
            var summaries = new List<YearSummary>();

            // Gennemgår hvert år
            foreach (var year in years)
            {
                // Finder alle ordrer for året
                var ordersForYear = _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .Where(o =>
                        o.SetupDate.HasValue &&
                        o.SetupDate.Value.Year == year)
                    .AsEnumerable();

                // Beregner omsætning
                // Pris + transport
                decimal revenue = ordersForYear
                    .Sum(o => o.Price + (o.Transport ?? 0));

                // Beregner samlede udgifter
                decimal expenses = _context.Expenses
                    .Where(e => e.Date.Year == year)
                    .Sum(e => e.TotalPrice);

                // Tæller antal jobs/ordrer
                int numberOfJobs = ordersForYear.Count();

                // Tilføjer årsopgørelse til liste
                summaries.Add(new YearSummary
                {
                    Year = year,

                    // Samlet omsætning
                    Revenue = revenue,

                    // Samlede udgifter
                    Expenses = expenses,

                    // Antal jobs
                    NumberOfJobs = numberOfJobs,

                    // Overskud = omsætning - udgifter
                    YearSummaries = revenue - expenses
                });
            }

            // Sender liste til view
            return View(summaries);
        }
    }
}