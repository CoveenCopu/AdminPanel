using AdminPanel.Data;
using AdminPanel.Models;
using FirebaseAdmin.Auth;
using Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers
{
    // Controller som håndterer login/logout
    public class AccountController : Controller
    {
        // Database context til adgang til SQL database
        private readonly AppDbContext _context;

        // Constructor som injicerer database context
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // Viser login siden
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Modtager login request fra frontend
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] TokenRequest request)
        {
            // Tjekker om token mangler
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
                // Verificerer Firebase token
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);

                // Henter Firebase UID fra token
                var uid = decodedToken.Uid;

                // Finder bruger i lokal SQL database via UID
                var user = _context.Users
                    .FirstOrDefault(u => u.Uid == uid);

                // Hvis bruger ikke findes i lokal database
                if (user == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Bruger findes ikke i lokal database"
                    });
                }

                // Henter email fra Firebase token
                var email = decodedToken.Claims.ContainsKey("email")
                    ? decodedToken.Claims["email"]?.ToString() ?? ""
                    : "";

                // Opretter claims til cookie authentication
                var claims = new List<Claim>
                {
                    // Unikt bruger-ID
                    new Claim(ClaimTypes.NameIdentifier, uid),

                    // Brugerens email
                    new Claim(ClaimTypes.Email, email),

                    // Navn på bruger (her bruges email)
                    new Claim(ClaimTypes.Name, email),

                    // Rolle fra SQL database (Admin/User)
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                // Opretter identity til cookie auth
                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                // Opretter principal ud fra identity
                var principal = new ClaimsPrincipal(identity);

                // Logger bruger ind via cookie authentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                // Returnerer success til frontend
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
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Fjerner login cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            // Sender bruger tilbage til login siden
            return RedirectToAction("Login", "Account");
        }
    }

    // Model som modtager Firebase token fra frontend
    public class TokenRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }
}