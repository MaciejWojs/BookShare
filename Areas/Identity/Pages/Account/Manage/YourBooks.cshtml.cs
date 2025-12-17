using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookShare.Data;
using Microsoft.EntityFrameworkCore;

namespace BookShare.Areas.Identity.Pages.Account.Manage
{
    public class YourBooksModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public YourBooksModel(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public List<UserBook> UserBooks { get; set; } = new List<UserBook>();
        public decimal TotalSpent { get; set; }
        public int CategoriesCount { get; set; }
        public int RecentPurchases { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Pobierz książki użytkownika z relacjami
            UserBooks = await _context.UserBooks
                .Where(ub => ub.UserId == user.Id)
                .Include(ub => ub.Book)
                .ThenInclude(b => b.Category)
                .OrderByDescending(ub => ub.AcquiredAt)
                .ToListAsync();

            // Oblicz statystyki
            if (UserBooks.Any())
            {
                TotalSpent = UserBooks.Sum(ub => ub.Book.Price);
                CategoriesCount = UserBooks
                    .Select(ub => ub.Book.CategoryId)
                    .Where(categoryId => categoryId.HasValue)
                    .Distinct()
                    .Count();
                
                var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
                RecentPurchases = UserBooks
                    .Count(ub => ub.AcquiredAt >= thirtyDaysAgo);
            }

            return Page();
        }
    }
}