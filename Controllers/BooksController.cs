using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookShare.Data;
using BookShare.Models;
using Microsoft.AspNetCore.Authorization;
using BookShare.Models.ViewModels;

namespace BookShare.Controllers {
    public class BooksController : Controller {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context) {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index() {
            var appDbContext = _context.Books.Include(b => b.Category);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create() {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId,CreatedAt")] Book book) {
            if (ModelState.IsValid) {
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var book = await _context.Books.FindAsync(id);
            if (book == null) {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")] // dokładnie ta nazwa
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId,CreatedAt")] Book NewBook) {
            if (id != NewBook.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    _context.Update(NewBook);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException) {
                    if (!BookExists(NewBook.Id)) {
                        return NotFound();
                    }
                    else {
                        throw;
                    }
                }
                return RedirectToAction("Manage", "Books");
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", NewBook.CategoryId);
            return View(NewBook);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            var book = await _context.Books.FindAsync(id);
            if (book != null) {
                _context.Books.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Manage", "Books");
        }

        //GET : Books/Manage
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Manage() {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            var model = new BookManagementViewModel {
                Books = await _context.Books.Include(b => b.Category).ToListAsync(),
                NewBook = new Book()              // pusty model dla formularza
            };

            return View(model);
        }
        //POST : Books/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Manage([Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId")] Book book) {
            Console.WriteLine("Dodawanie książki: " + book.Title);
            book.CreatedAt = DateTime.UtcNow; // Set creation date automatically

            if (ModelState.IsValid) {
                Console.WriteLine("Model jest poprawny.");
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Manage));
            }

            // If we get here, something failed, redisplay form
            Console.WriteLine("Model jest niepoprawny.");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
            var model = new BookManagementViewModel {
                Books = await _context.Books.Include(b => b.Category).ToListAsync(),
                NewBook = book              // zwracamy wypełniony model w przypadku błędu
            };

            return View(model);
        }



        private bool BookExists(int id) {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
