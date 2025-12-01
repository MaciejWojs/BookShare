using System.Diagnostics;
using BookShare.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BookShare.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShare.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, AppDbContext context) {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> IndexAsync() {
            // Fetch books with their categories from database
            var books = await _context.Books
                .Include(b => b.Category)
                .Where(b => b.StockQuantity > 0) // Only show books in stock
                .OrderByDescending(b => b.CreatedAt)
                .Take(20) // Limit to 20 most recent books
                .ToListAsync();

            return View(books);
        }

        [Authorize(Roles = "Administrator")] // dokładnie ta nazwa
        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        [Authorize(Roles = "Administrator")]
        public IActionResult AdminPanel() {
            // var viewModel = new AdminPanelViewModel();
            // return View(viewModel);
            return View();
        }
    }
}
