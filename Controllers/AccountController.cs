using AdminPanel.Data;
using AdminPanel.Models;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        // Database context
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Viser login-side
        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Logger bruger ind med Firebase ID token
        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] FirebaseTokenRequest request)
        {
            // Tjekker om token er tom eller mangler
            if (string.IsNullOrWhiteSpace(request?.IdToken))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Manglende token"
                });
            }

            try
            {
                // Verificerer Firebase ID token
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);

                // Henter brugerens UID fra Firebase
                var uid = decodedToken.Uid;

                // Finder bruger i lokal database
                var user = _context.Users
                    .FirstOrDefault(u => u.Uid == uid);

                // Hvis bruger ikke findes i systemet
                if (user == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Bruger findes ikke i lokal database"
                    });
                }

                // Henter email fra Firebase claims
                var email = decodedToken.Claims.ContainsKey("email")
                    ? decodedToken.Claims["email"]?.ToString() ?? ""
                    : "";

                // Opretter claims til cookie authentication
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, uid),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                // Opretter identity baseret på claims
                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                // Opretter principal (bruger)
                var principal = new ClaimsPrincipal(identity);

                // Logger brugeren ind (cookie authentication)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                // Returnerer succes og brugerens rolle
                return Ok(new
                {
                    success = true,
                    role = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                // Returnerer fejl hvis login fejler
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // Logger bruger ud
        // POST: Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Fjerner cookie authentication
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Sender bruger tilbage til login side
            return RedirectToAction("Login", "Account");
        }
    }
}