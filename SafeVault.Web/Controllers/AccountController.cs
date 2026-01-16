using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SafeVault.Web.Services;
using System.Security.Claims;

namespace SafeVault.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly LoginService _loginService;

        public AccountController(LoginService loginService)
        {
            _loginService = loginService;
        }

        // Show login page
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (_loginService.AuthenticateUser(username, password, out string role))
            {
                // 1️⃣ Create claims for cookie auth
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role) // This enables [Authorize(Roles="Admin")]
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // 2️⃣ Sign in user with cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // 3️⃣ Optional: store session data
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetString("UserRole", role);

                return RedirectToAction("Index", "Home");
            }

            ViewData["Error"] = "Invalid username or password!";
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();

            // Sign out cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Delete JWT cookie
            Response.Cookies.Delete("JwtToken");

            return RedirectToAction("Index", "Home");
        }

        // Show registration page
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string username, string password, string confirmPassword, string role = "User")
        {
            // Validate passwords
            if (password != confirmPassword)
            {
                ViewBag.Message = "Passwords do not match!";
                return View();
            }

            // Ensure role is either Admin or User
            role = role == "Admin" ? "Admin" : "User";

            // Attempt registration
            if (_loginService.RegisterUser(username, password, role))
            {
                ViewBag.Message = $"Registration successful! You registered as {role}. Please login.";
                return View();
            }

            ViewBag.Message = "Username already exists or invalid input.";
            return View();
        }
    }
}
