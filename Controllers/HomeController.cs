using System.Diagnostics;
using BookShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookShare.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager; // 👈 dodajemy UserManager


        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager) {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> IndexAsync() {
            // var user = await _userManager.GetUserAsync(User);
            // var roles = await _userManager.GetRolesAsync(user);

            // Console.WriteLine("ROLES :" + string.Join(", ", roles));
            return View();
        }

        [Authorize(Roles = "Administrator")] // dokładnie ta nazwa
        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
