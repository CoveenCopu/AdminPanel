using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    // Kun brugere som er logget ind må få adgang
    [Authorize]
    public class HomeController : Controller
    {
        // Viser forsiden/dashboard
        public IActionResult Index()
        {
            return View();
        }
    }
}