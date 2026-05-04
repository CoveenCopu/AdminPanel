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
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] TokenRequest request)
        {
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
                // Verificer Firebase token
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);

                var uid = decodedToken.Uid;

                // Hent bruger fra lokal SQL database via UID
                var user = _context.Users
                    .FirstOrDefault(u => u.Uid == uid);

                if (user == null)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Bruger findes ikke i lokal database"
                    });
                }

                // Hent email fra Firebase token
                var email = decodedToken.Claims.ContainsKey("email")
                    ? decodedToken.Claims["email"]?.ToString() ?? ""
                    : "";

                // Opret claims inkl. Role fra SQL database
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, uid),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, email),

                    // Rolle fra lokal database
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                // Login med cookie authentication
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return Ok(new
                {
                    success = true,
                    role = user.Role.ToString()
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
    }

    public class TokenRequest
    {
        public string IdToken { get; set; } = string.Empty;
    }
}