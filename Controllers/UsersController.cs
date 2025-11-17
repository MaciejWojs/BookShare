using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookShare.Data;
using BookShare.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookShare.Controllers {
    public class UsersController : Controller {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger) {
            _context = context;
            _logger = logger;
        }

        // GET: Users
        public async Task<IActionResult> Index() {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(string id) {
            if (id == null) {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id) {
            if (id == null) {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Username,Role,CreatedAt,Balance,Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] User user) {
            if (id != user.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!UserExists(user.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id) {
            if (id == null) {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id) {
            var user = await _context.Users.FindAsync(id);
            if (user != null) {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Post: Users/Promote/5
        [HttpPost, ActionName("Promote")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PromoteConfirmed(string id) {
            _logger.LogInformation("Promoting user with ID: {Id}", id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId) {
                TempData["Error"] = "Nie możesz promować samego siebie.";
                _logger.LogWarning("User {UserId} attempted to promote themselves.", currentUserId);
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null) {
                if (user.Role == UserRole.Administrator) {
                    _logger.LogInformation("User {Id} is already Administrator — skipping.", id);
                    return RedirectToAction(nameof(Index));
                }

                user.Role = UserRole.Administrator;
                _context.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Id} promoted to Administrator.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        // Post: Users/Demote/5
        [HttpPost, ActionName("Demote")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DemoteConfirmed(string id) {
            _logger.LogInformation("Demoting user with ID: {Id}", id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId) {
                TempData["Error"] = "Nie możesz zdegradować samego siebie.";
                _logger.LogWarning("User {UserId} attempted to demote themselves.", currentUserId);
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null) {
                if (user.Role == UserRole.Customer) {
                    _logger.LogInformation("User {Id} is already Customer — skipping.", id);
                    return RedirectToAction(nameof(Index));
                }

                user.Role = UserRole.Customer;
                _context.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Id} demoted to Customer.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id) {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
