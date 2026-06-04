using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var totalBooks = await _context.Books.CountAsync();
            var activeMembers = await _context.Members.CountAsync(m => m.IsActive);
            var activeBorrowings = await _context.Borrowings.CountAsync(b => b.Status == "Borrowed");
            var revenue = await _context.Borrowings.SumAsync(b => b.LateFee);

            var categories = await _context.Books
                .GroupBy(b => b.Category)
                .Select(g => g.Key)
                .Take(6)
                .ToListAsync();

            var categoryCounts = await _context.Books
                .GroupBy(b => b.Category)
                .Select(g => g.Count())
                .Take(6)
                .ToListAsync();

            var months = Enumerable.Range(1, 6)
                .Select(i => DateTime.Now.AddMonths(-i).ToString("MMM"))
                .Reverse()
                .ToList();

            var borrowingTrends = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var count = await _context.Borrowings
                    .CountAsync(b => b.BorrowDate.Year == date.Year && b.BorrowDate.Month == date.Month);
                borrowingTrends.Add(count);
            }

            return Json(new
            {
                totalBooks,
                activeMembers,
                activeBorrowings,
                revenue = revenue.ToString("F2"),
                categories,
                categoryCounts,
                months,
                borrowingTrends
            });
        }
    }
}