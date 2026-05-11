using AdminPanel.Data;
using AdminPanel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Controllers
{
    // Kun admins må bruge Users controller
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // Henter og viser alle brugere
        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();

            return View(users);
        }

        // Viser formular til oprettelse af bruger
        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // Opretter ny bruger
        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name, Email, Role")] User user)
        {
            // Tjekker om model er valid
            if (ModelState.IsValid)
            {
                // Tilføjer bruger til database
                _context.Add(user);

                // Gemmer ændringer i SQL database
                await _context.SaveChangesAsync();

                // Sender bruger tilbage til Index siden
                return RedirectToAction(nameof(Index));
            }

            // Returnerer view igen hvis model ikke er valid
            return View(user);
        }
    }
}