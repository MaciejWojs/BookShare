using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShare.Data;
using BookShare.Models;
using BookShare.Models.ViewModels;

namespace BookShare.Controllers {
    public class CategoriesController : Controller {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context) {
            _context = context;
        }

        // GET: Categories/Manage
        public async Task<IActionResult> Manage() {
            var viewModel = new CategoryManagementViewModel {
                Categories = await _context.Categories
                    .Include(c => c.Books)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Categories/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(CategoryManagementViewModel viewModel) {
            if (ModelState.IsValid && viewModel.NewCategory != null) {
                // Ensure CreatedAt is set
                viewModel.NewCategory.CreatedAt = DateTimeOffset.UtcNow;
                _context.Add(viewModel.NewCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }

            // Reload categories if model is invalid
            viewModel.Categories = await _context.Categories
                .Include(c => c.Books)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View(viewModel);
        }

        // GET: Categories
        public async Task<IActionResult> Index() {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create() {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CreatedAt")] Category category) {
            if (ModelState.IsValid) {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Manage","Categories");
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null) {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedAt")] Category category) {
            if (id != category.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!CategoryExists(category.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction("Manage","Categories");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var category = await _context.Categories.FindAsync(id);
            if (category != null) {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", "Books");
        }

        private bool CategoryExists(int id) {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
