using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Members
        public async Task<IActionResult> Index(string searchString, bool? isActive)
        {
            var members = from m in _context.Members select m;

            // Search functionality
            if (!string.IsNullOrEmpty(searchString))
            {
                members = members.Where(m => m.FullName.Contains(searchString) ||
                                            m.Email.Contains(searchString) ||
                                            m.MembershipNumber.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Filter by active status
            if (isActive.HasValue)
            {
                members = members.Where(m => m.IsActive == isActive.Value);
                ViewBag.IsActive = isActive;
            }

            return View(await members.ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            // Get borrowing history for this member
            var borrowingHistory = await _context.Borrowings
                .Include(b => b.Book)
                .Where(b => b.MemberId == id)
                .OrderByDescending(b => b.BorrowDate)
                .ToListAsync();

            ViewBag.BorrowingHistory = borrowingHistory;

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            // Generate membership number automatically
            var lastMember = _context.Members.OrderByDescending(m => m.Id).FirstOrDefault();
            string newMembershipNumber = "MEM" + DateTime.Now.Year.ToString();

            if (lastMember == null)
            {
                newMembershipNumber += "0001";
            }
            else
            {
                int lastNumber = int.Parse(lastMember.MembershipNumber.Substring(7));
                newMembershipNumber += (lastNumber + 1).ToString("D4");
            }

            ViewBag.MembershipNumber = newMembershipNumber;
            return View();
        }

        // POST: Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MembershipNumber,FullName,Email,Phone,Address,RegistrationDate,IsActive")] Member member)
        {
            if (ModelState.IsValid)
            {
                member.RegistrationDate = DateTime.Now;
                _context.Add(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MembershipNumber,FullName,Email,Phone,Address,RegistrationDate,IsActive")] Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Member updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            // Check if member has active borrowings
            var activeBorrowings = await _context.Borrowings
                .Where(b => b.MemberId == id && b.Status == "Borrowed")
                .AnyAsync();

            if (activeBorrowings)
            {
                TempData["Error"] = "Cannot delete member with active borrowings!";
                return RedirectToAction(nameof(Index));
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}