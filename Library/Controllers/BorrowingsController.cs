using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class BorrowingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BorrowingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Borrowings
        public async Task<IActionResult> Index(string status, string searchString)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                borrowings = borrowings.Where(b => b.Status == status);
                ViewBag.CurrentStatus = status;
            }

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                borrowings = borrowings.Where(b => b.Member.FullName.Contains(searchString) ||
                                                  b.Book.Title.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            return View(await borrowings.OrderByDescending(b => b.BorrowDate).ToListAsync());
        }

        // GET: Borrowings/Create
        // GET: Borrowings/Create
        public async Task<IActionResult> Create()
        {
            // Load available books and active members
            ViewBag.Books = await _context.Books
                .Where(b => b.AvailableCopies > 0)
                .Select(b => new { b.Id, b.Title, b.AvailableCopies })
                .ToListAsync();

            ViewBag.Members = await _context.Members
                .Where(m => m.IsActive)
                .Select(m => new { m.Id, m.FullName })
                .ToListAsync();

            var borrowing = new Borrowing
            {
                BorrowDate = DateTime.Now,
                ExpectedReturnDate = DateTime.Now.AddDays(14)
            };

            return View(borrowing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Borrowing borrowing)  // Remove [Bind] attribute
        {
            Console.WriteLine("POST HIT");

            // Remove ModelState errors for navigation properties
            ModelState.Remove("Book");
            ModelState.Remove("Member");

            // Set borrow date
            borrowing.BorrowDate = DateTime.Now;

            // Set default expected return date if not provided
            if (borrowing.ExpectedReturnDate == default(DateTime))
            {
                borrowing.ExpectedReturnDate = DateTime.Now.AddDays(14);
            }

            // Validate dates
            if (borrowing.ExpectedReturnDate <= borrowing.BorrowDate)
            {
                ModelState.AddModelError("ExpectedReturnDate", "Expected return date must be after borrow date");
            }

            // Check if ModelState is valid
            if (!ModelState.IsValid)
            {
                // Reload dropdowns
                ViewBag.Books = await _context.Books
                    .Where(b => b.AvailableCopies > 0)
                    .Select(b => new { b.Id, b.Title, b.AvailableCopies })
                    .ToListAsync();

                ViewBag.Members = await _context.Members
                    .Where(m => m.IsActive)
                    .Select(m => new { m.Id, m.FullName })
                    .ToListAsync();

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(borrowing);
            }

            // Check if book is available
            var book = await _context.Books.FindAsync(borrowing.BookId);
            if (book == null)
            {
                TempData["Error"] = "Book not found!";
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
                ViewBag.Members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                return View(borrowing);
            }

            if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError("BookId", "This book is not available for borrowing!");
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
                ViewBag.Members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                return View(borrowing);
            }

            // Check if member is active
            var member = await _context.Members.FindAsync(borrowing.MemberId);
            if (member == null || !member.IsActive)
            {
                ModelState.AddModelError("MemberId", "Member is not active or not found!");
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
                ViewBag.Members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                return View(borrowing);
            }

            // Check if member has any overdue books
            var hasOverdue = await _context.Borrowings
                .AnyAsync(b => b.MemberId == borrowing.MemberId &&
                              b.Status == "Borrowed" &&
                              b.ExpectedReturnDate < DateTime.Now);

            if (hasOverdue)
            {
                TempData["Error"] = "Member has overdue books! Please return them first.";
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
                ViewBag.Members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                return View(borrowing);
            }

            // Check max books per member (optional - limit 5 books)
            var currentBorrowings = await _context.Borrowings
                .CountAsync(b => b.MemberId == borrowing.MemberId && b.Status == "Borrowed");

            if (currentBorrowings >= 5)
            {
                TempData["Error"] = "Member already has 5 borrowed books! Maximum limit reached.";
                ViewBag.Books = await _context.Books.Where(b => b.AvailableCopies > 0).ToListAsync();
                ViewBag.Members = await _context.Members.Where(m => m.IsActive).ToListAsync();
                return View(borrowing);
            }

            // Create the borrowing record
            borrowing.Status = "Borrowed";
            _context.Add(borrowing);

            // Update available copies
            book.AvailableCopies--;
            _context.Update(book);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Book '{book.Title}' borrowed successfully by {member.FullName}!";
            return RedirectToAction(nameof(Index));
        }
        // GET: Borrowings/Return/5
        public async Task<IActionResult> Return(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (borrowing == null)
            {
                return NotFound();
            }

            // Calculate late fee if any
            if (borrowing.ExpectedReturnDate < DateTime.Now)
            {
                int daysLate = (DateTime.Now - borrowing.ExpectedReturnDate).Days;
                borrowing.LateFee = daysLate * 5; // $5 per day late
            }

            return View(borrowing);
        }

        // POST: Borrowings/Return/5
        [HttpPost, ActionName("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnConfirmed(int id)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (borrowing == null)
            {
                return NotFound();
            }

            borrowing.ActualReturnDate = DateTime.Now;
            borrowing.Status = "Returned";

            // Update available copies
            var book = await _context.Books.FindAsync(borrowing.BookId);
            if (book != null)
            {
                book.AvailableCopies++;
                _context.Update(book);
            }

            _context.Update(borrowing);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Book returned successfully! Late fee: ${borrowing.LateFee}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Borrowings/History
        public async Task<IActionResult> History(int? memberId)
        {
            var borrowings = _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Member)
                .AsQueryable();

            if (memberId.HasValue)
            {
                borrowings = borrowings.Where(b => b.MemberId == memberId.Value);
                ViewBag.MemberName = (await _context.Members.FindAsync(memberId.Value))?.FullName;
            }

            return View(await borrowings.OrderByDescending(b => b.BorrowDate).ToListAsync());
        }
    }
}