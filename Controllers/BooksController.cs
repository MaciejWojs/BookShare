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
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace BookShare.Controllers {
    public class BooksController : Controller {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(AppDbContext context, IWebHostEnvironment webHostEnvironment) {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create([Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId,CreatedAt")] Book book, IFormFile pdfFile, IFormFile coverImage) {
            if (ModelState.IsValid) {
                // Obsługa pliku PDF
                if (pdfFile != null && pdfFile.Length > 0) {
                    // Sprawdź czy plik jest PDF
                    if (pdfFile.ContentType != "application/pdf") {
                        ModelState.AddModelError("pdfFile", "Można przesyłać tylko pliki PDF.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                        return View(book);
                    }

                    // Utwórz nazwę pliku z unikalnym identyfikatorem
                    var pdfFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                    var pdfUploadsPath = Path.Combine("/app/uploads", "books");
                    
                    // Utwórz katalog jeśli nie istnieje
                    if (!Directory.Exists(pdfUploadsPath)) {
                        Directory.CreateDirectory(pdfUploadsPath);
                    }

                    var pdfFilePath = Path.Combine(pdfUploadsPath, pdfFileName);

                    // Zapisz plik
                    using (var fileStream = new FileStream(pdfFilePath, FileMode.Create)) {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    book.PdfFilePath = Path.Combine("books", pdfFileName);
                }

                // Obsługa obrazu okładki
                if (coverImage != null && coverImage.Length > 0) {
                    // Sprawdź czy plik jest obrazem
                    var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedImageTypes.Contains(coverImage.ContentType.ToLower())) {
                        ModelState.AddModelError("coverImage", "Można przesyłać tylko pliki obrazów (JPG, PNG, GIF, WebP).");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                        return View(book);
                    }

                    // Utwórz nazwę pliku z unikalnym identyfikatorem
                    var imageFileName = $"{Guid.NewGuid()}_{coverImage.FileName}";
                    var imageUploadsPath = Path.Combine("/app/uploads", "covers");
                    
                    // Utwórz katalog jeśli nie istnieje
                    if (!Directory.Exists(imageUploadsPath)) {
                        Directory.CreateDirectory(imageUploadsPath);
                    }

                    var imageFilePath = Path.Combine(imageUploadsPath, imageFileName);

                    // Zapisz plik
                    using (var fileStream = new FileStream(imageFilePath, FileMode.Create)) {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    book.CoverImagePath = Path.Combine("covers", imageFileName);
                }

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId,CreatedAt,PdfFilePath,CoverImagePath")] Book NewBook, IFormFile pdfFile, IFormFile coverImage) {
            if (id != NewBook.Id) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    // Jeśli nowy plik PDF został przesłany
                    if (pdfFile != null && pdfFile.Length > 0) {
                        // Sprawdź czy plik jest PDF
                        if (pdfFile.ContentType != "application/pdf") {
                            ModelState.AddModelError("pdfFile", "Można przesyłać tylko pliki PDF.");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", NewBook.CategoryId);
                            return View(NewBook);
                        }

                        // Usuń stary plik jeśli istnieje
                        if (!string.IsNullOrEmpty(NewBook.PdfFilePath)) {
                            var oldPdfPath = Path.Combine("/app/uploads", NewBook.PdfFilePath);
                            if (System.IO.File.Exists(oldPdfPath)) {
                                System.IO.File.Delete(oldPdfPath);
                            }
                        }

                        // Utwórz nazwę pliku z unikalnym identyfikatorem
                        var pdfFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                        var pdfUploadsPath = Path.Combine("/app/uploads", "books");
                        
                        // Utwórz katalog jeśli nie istnieje
                        if (!Directory.Exists(pdfUploadsPath)) {
                            Directory.CreateDirectory(pdfUploadsPath);
                        }

                        var pdfFilePath = Path.Combine(pdfUploadsPath, pdfFileName);

                        // Zapisz plik
                        using (var fileStream = new FileStream(pdfFilePath, FileMode.Create)) {
                            await pdfFile.CopyToAsync(fileStream);
                        }

                        NewBook.PdfFilePath = Path.Combine("books", pdfFileName);
                    }

                    // Jeśli nowy obraz okładki został przesłany
                    if (coverImage != null && coverImage.Length > 0) {
                        // Sprawdź czy plik jest obrazem
                        var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                        if (!allowedImageTypes.Contains(coverImage.ContentType.ToLower())) {
                            ModelState.AddModelError("coverImage", "Można przesyłać tylko pliki obrazów (JPG, PNG, GIF, WebP).");
                            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", NewBook.CategoryId);
                            return View(NewBook);
                        }

                        // Usuń stary obraz jeśli istnieje
                        if (!string.IsNullOrEmpty(NewBook.CoverImagePath)) {
                            var oldImagePath = Path.Combine("/app/uploads", NewBook.CoverImagePath);
                            if (System.IO.File.Exists(oldImagePath)) {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Utwórz nazwę pliku z unikalnym identyfikatorem
                        var imageFileName = $"{Guid.NewGuid()}_{coverImage.FileName}";
                        var imageUploadsPath = Path.Combine("/app/uploads", "covers");
                        
                        // Utwórz katalog jeśli nie istnieje
                        if (!Directory.Exists(imageUploadsPath)) {
                            Directory.CreateDirectory(imageUploadsPath);
                        }

                        var imageFilePath = Path.Combine(imageUploadsPath, imageFileName);

                        // Zapisz plik
                        using (var fileStream = new FileStream(imageFilePath, FileMode.Create)) {
                            await coverImage.CopyToAsync(fileStream);
                        }

                        NewBook.CoverImagePath = Path.Combine("covers", imageFileName);
                    }

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
        public async Task<IActionResult> Manage([Bind("Id,Title,Author,ISBN,Description,Price,StockQuantity,CategoryId")] Book book, IFormFile pdfFile, IFormFile coverImage) {
            Console.WriteLine("Dodawanie książki: " + book.Title);
            book.CreatedAt = DateTime.UtcNow; // Set creation date automatically

            if (ModelState.IsValid) {
                Console.WriteLine("Model jest poprawny.");
                
                // Obsługa pliku PDF
                if (pdfFile != null && pdfFile.Length > 0) {
                    // Sprawdź czy plik jest PDF
                    if (pdfFile.ContentType != "application/pdf") {
                        ModelState.AddModelError("pdfFile", "Można przesyłać tylko pliki PDF.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                        var errorModel = new BookManagementViewModel {
                            Books = await _context.Books.Include(b => b.Category).ToListAsync(),
                            NewBook = book
                        };
                        return View(errorModel);
                    }

                    // Utwórz nazwę pliku z unikalnym identyfikatorem
                    var pdfFileName = $"{Guid.NewGuid()}_{pdfFile.FileName}";
                    var pdfUploadsPath = Path.Combine("/app/uploads", "books");
                    
                    // Utwórz katalog jeśli nie istnieje
                    if (!Directory.Exists(pdfUploadsPath)) {
                        Directory.CreateDirectory(pdfUploadsPath);
                    }

                    var pdfFilePath = Path.Combine(pdfUploadsPath, pdfFileName);

                    // Zapisz plik
                    using (var fileStream = new FileStream(pdfFilePath, FileMode.Create)) {
                        await pdfFile.CopyToAsync(fileStream);
                    }

                    book.PdfFilePath = Path.Combine("books", pdfFileName);
                }

                // Obsługa obrazu okładki
                if (coverImage != null && coverImage.Length > 0) {
                    // Sprawdź czy plik jest obrazem
                    var allowedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                    if (!allowedImageTypes.Contains(coverImage.ContentType.ToLower())) {
                        ModelState.AddModelError("coverImage", "Można przesyłać tylko pliki obrazów (JPG, PNG, GIF, WebP).");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", book.CategoryId);
                        var errorModel = new BookManagementViewModel {
                            Books = await _context.Books.Include(b => b.Category).ToListAsync(),
                            NewBook = book
                        };
                        return View(errorModel);
                    }

                    // Utwórz nazwę pliku z unikalnym identyfikatorem
                    var imageFileName = $"{Guid.NewGuid()}_{coverImage.FileName}";
                    var imageUploadsPath = Path.Combine("/app/uploads", "covers");
                    
                    // Utwórz katalog jeśli nie istnieje
                    if (!Directory.Exists(imageUploadsPath)) {
                        Directory.CreateDirectory(imageUploadsPath);
                    }

                    var imageFilePath = Path.Combine(imageUploadsPath, imageFileName);

                    // Zapisz plik
                    using (var fileStream = new FileStream(imageFilePath, FileMode.Create)) {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    book.CoverImagePath = Path.Combine("covers", imageFileName);
                }

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

        // POST: Books/Purchase/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Purchase(int id) {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) {
                TempData["Error"] = "Musisz być zalogowany, aby kupować książki.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(userId);
            var book = await _context.Books.FindAsync(id);

            if (user == null || book == null) {
                TempData["Error"] = "Nie znaleziono użytkownika lub książki.";
                return RedirectToAction(nameof(Index));
            }

            // Sprawdź czy użytkownik już ma tę książkę
            var existingUserBook = await _context.UserBooks
                .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == id);
            
            if (existingUserBook != null) {
                TempData["Error"] = "Już posiadasz tę książkę!";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Sprawdź stan magazynowy
            if (book.StockQuantity <= 0) {
                TempData["Error"] = "Książka jest niedostępna w magazynie.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Sprawdź czy użytkownik ma wystarczająco środków
            if (user.Balance < book.Price) {
                TempData["Error"] = $"Niewystarczające środki. Potrzebujesz {book.Price} PLN, a masz {user.Balance} PLN.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Wykonaj transakcję
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                // Odejmij środki od użytkownika
                user.Balance -= book.Price;

                // Zmniejsz stan magazynowy
                book.StockQuantity -= 1;

                // Utwórz zamówienie
                var order = new Order {
                    UserId = userId,
                    OrderNumber = new Random().Next(100000, 999999), // Prosty generator numeru zamówienia
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = book.Price,
                    Status = OrderStatus.Completed
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Zapisz zamówienie, aby uzyskać Id

                // Utwórz element zamówienia
                var orderItem = new OrderItem {
                    OrderId = order.Id,
                    BookId = book.Id,
                    Quantity = 1,
                    UnitPrice = book.Price
                };

                _context.OrderItems.Add(orderItem);

                // Dodaj książkę do biblioteki użytkownika
                var userBook = new UserBook {
                    UserId = userId,
                    BookId = id,
                    AcquiredAt = DateTimeOffset.UtcNow,
                    Notes = $"Zakupiona za {book.Price} PLN"
                };

                _context.UserBooks.Add(userBook);

                // Zapisz wszystkie zmiany
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Pomyślnie zakupiłeś książkę '{book.Title}' za {book.Price} PLN!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                TempData["Error"] = "Wystąpił błąd podczas zakupu. Spróbuj ponownie.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // GET: Books/DownloadPdf/5
        [Authorize]
        public async Task<IActionResult> DownloadPdf(int id) {
            var book = await _context.Books.FindAsync(id);
            if (book == null || string.IsNullOrEmpty(book.PdfFilePath)) {
                return NotFound("Książka lub plik PDF nie zostały znalezione.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Sprawdź czy użytkownik ma tę książkę (administratorzy mają dostęp do wszystkich)
            if (!User.IsInRole("Administrator")) {
                var hasBook = await _context.UserBooks
                    .AnyAsync(ub => ub.UserId == userId && ub.BookId == id);
                
                if (!hasBook) {
                    return Forbid("Nie masz dostępu do tej książki. Musisz ją najpierw kupić.");
                }
            }

            var filePath = Path.Combine("/app/uploads", book.PdfFilePath);
            if (!System.IO.File.Exists(filePath)) {
                return NotFound("Plik PDF nie istnieje.");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = $"{book.Title}_{book.Author}.pdf";

            return File(fileBytes, "application/pdf", fileName);
        }

        // GET: Books/GetCoverImage/5
        public async Task<IActionResult> GetCoverImage(int id) {
            var book = await _context.Books.FindAsync(id);
            if (book == null || string.IsNullOrEmpty(book.CoverImagePath)) {
                // Zwróć placeholder image
                return Redirect("https://commons.wikimedia.org/wiki/File:No-Image-Placeholder.svg");
            }

            var filePath = Path.Combine("/app/uploads", book.CoverImagePath);
            if (!System.IO.File.Exists(filePath)) {
                // Zwróć placeholder image
                return Redirect("https://commons.wikimedia.org/wiki/File:No-Image-Placeholder.svg");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(filePath);

            return File(fileBytes, contentType);
        }

        private string GetContentType(string path) {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png", 
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }



        private bool BookExists(int id) {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
