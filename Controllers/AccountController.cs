using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] TokenRequest request)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(request.IdToken);

                var email = decodedToken.Claims.ContainsKey("email")
                    ? decodedToken.Claims["email"].ToString()
                    : "";

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, decodedToken.Uid),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Name, email)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal);

                return Ok(new { success = true });
            }
            catch
            {
                return Unauthorized(new { success = false });
            }
        }

        // LOG UD FUNKTION
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
        public string IdToken { get; set; }
    }
}