using System.Threading.Tasks;
using BookShare.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BookShare.Data;

namespace BookShare.Areas.Identity.Pages.Account.Manage
{
    public class BalanceModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public BalanceModel(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public decimal CurrentBalance { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CurrentBalance = user.Balance;
            return Page();
        }

        public async Task<IActionResult> OnPostAddBalanceAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            user.Balance += 100m;
            _context.Update(user);
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully added 100 PLN to your account!";
            return RedirectToPage();
        }
    }
}