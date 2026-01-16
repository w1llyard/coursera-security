using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SafeVault.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Only users with Admin role can access
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            // No need to manually check session or JWT
            return View();
        }
    }
}
