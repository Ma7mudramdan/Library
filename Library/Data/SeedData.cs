using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // ============================================
                // 1. CREATE ROLES
                // ============================================
                string[] roleNames = { "Admin", "Librarian", "User" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        Console.WriteLine($"✅ Role '{roleName}' created");
                    }
                }

                // ============================================
                // 2. CREATE ADMIN USER
                // ============================================
                var adminEmail = "admin@libraryhub.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FirstName = "System",
                        LastName = "Administrator",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        PhoneNumber = "+20123456789",
                        Address = "Admin Office, Main Library Building"
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine("✅ Admin user created");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Error creating admin: {error.Description}");
                        }
                    }
                }

                // ============================================
                // 3. CREATE LIBRARIAN USER
                // ============================================
                var librarianEmail = "librarian@libraryhub.com";
                var librarianUser = await userManager.FindByEmailAsync(librarianEmail);

                if (librarianUser == null)
                {
                    librarianUser = new ApplicationUser
                    {
                        UserName = librarianEmail,
                        Email = librarianEmail,
                        FirstName = "Library",
                        LastName = "Staff",
                        EmailConfirmed = true,
                        CreatedAt = DateTime.Now,
                        IsActive = true,
                        PhoneNumber = "+20198765432",
                        Address = "Librarian Desk, Library Building"
                    };

                    var result = await userManager.CreateAsync(librarianUser, "Librarian@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(librarianUser, "Librarian");
                        Console.WriteLine("✅ Librarian user created");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Error creating librarian: {error.Description}");
                        }
                    }
                }

                // ============================================
                // 4. CREATE REGULAR USERS (20 sample users)
                // ============================================
                var random = new Random();
                for (int i = 1; i <= 20; i++)
                {
                    var userEmail = $"user{i}@libraryhub.com";
                    var existingUser = await userManager.FindByEmailAsync(userEmail);

                    if (existingUser == null)
                    {
                        var firstName = GetRandomFirstName();
                        var lastName = GetRandomLastName();

                        var user = new ApplicationUser
                        {
                            UserName = userEmail,
                            Email = userEmail,
                            FirstName = firstName,
                            LastName = lastName,
                            EmailConfirmed = true,
                            CreatedAt = DateTime.Now.AddDays(-i * 5),
                            IsActive = i % 5 != 0, // 80% active
                            PhoneNumber = $"+201{random.Next(10000000, 99999999)}",
                            Address = $"{random.Next(1, 999)} {GetRandomStreet()}, Cairo, Egypt"
                        };

                        var result = await userManager.CreateAsync(user, "User@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "User");
                        }
                    }
                }
                Console.WriteLine("✅ 20 regular users created");

                // ============================================
                // 5. SEED BOOKS (200 books)
                // ============================================
                if (!context.Books.Any())
                {
                    var books = GenerateBooks();
                    await context.Books.AddRangeAsync(books);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {books.Count} books added");
                }

                // ============================================
                // 6. SEED MEMBERS (50 members)
                // ============================================
                if (!context.Members.Any())
                {
                    var members = GenerateMembers();
                    await context.Members.AddRangeAsync(members);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {members.Count} members added");
                }

                // ============================================
                // 7. SEED BORROWINGS (100 borrowings)
                // ============================================
                if (!context.Borrowings.Any())
                {
                    var borrowings = await GenerateBorrowings(context);
                    await context.Borrowings.AddRangeAsync(borrowings);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"✅ {borrowings.Count} borrowings added");
                }
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private static List<Book> GenerateBooks()
        {
            var books = new List<Book>();
            var random = new Random();

            var titles = new[]
            {
                "Clean Code", "The Pragmatic Programmer", "Design Patterns", "Introduction to Algorithms",
                "The Mythical Man-Month", "Code Complete", "Refactoring", "You Don't Know JS",
                "Eloquent JavaScript", "Python Crash Course", "The Selfish Gene", "Sapiens",
                "Homo Deus", "Thinking Fast and Slow", "The Art of War", "The 7 Habits of Highly Effective People",
                "How to Win Friends and Influence People", "Atomic Habits", "Dune", "1984",
                "To Kill a Mockingbird", "The Great Gatsby", "Pride and Prejudice", "The Hobbit",
                "The Lord of the Rings", "Harry Potter", "The Da Vinci Code", "The Alchemist"
            };

            var authors = new[]
            {
                "Robert C. Martin", "Andy Hunt", "Dave Thomas", "Erich Gamma", "Thomas H. Cormen",
                "Frederick P. Brooks Jr.", "Steve McConnell", "Martin Fowler", "Kyle Simpson",
                "Marijn Haverbeke", "Richard Dawkins", "Yuval Noah Harari", "Daniel Kahneman",
                "Sun Tzu", "Stephen R. Covey", "Dale Carnegie", "James Clear", "Frank Herbert",
                "George Orwell", "Harper Lee", "F. Scott Fitzgerald", "Jane Austen", "J.R.R. Tolkien",
                "J.K. Rowling", "Dan Brown", "Paulo Coelho"
            };

            var categories = new[] { "Programming", "Science", "History", "Self-Help", "Business", "Fiction", "Fantasy" };

            for (int i = 1; i <= 200; i++)
            {
                var title = titles[random.Next(titles.Length)];
                if (i > titles.Length)
                {
                    title = $"{title} Vol. {random.Next(1, 5)}";
                }

                var copies = random.Next(1, 15);

                books.Add(new Book
                {
                    Id = i,
                    Title = title,
                    Author = authors[random.Next(authors.Length)],
                    Category = categories[random.Next(categories.Length)],
                    ISBN = $"978-{random.Next(100, 999)}-{random.Next(10000, 99999)}-{random.Next(0, 9)}",
                    Copies = copies,
                    AvailableCopies = random.Next(0, copies + 1),
                    PublishDate = DateTime.Now.AddYears(-random.Next(0, 50)),
                    Price = Math.Round((decimal)(random.Next(1500, 8000) / 100.0), 2),
                    Description = $"This is a comprehensive guide to {title}. A must-read for everyone interested in {categories[random.Next(categories.Length)]}."
                });
            }

            return books;
        }

        private static List<Member> GenerateMembers()
        {
            var members = new List<Member>();
            var random = new Random();

            var firstNames = new[]
            {
                "Ahmed", "Mohamed", "Ali", "Hassan", "Omar", "Khaled", "Youssef", "Ibrahim",
                "Mahmoud", "Tarek", "Sara", "Fatima", "Aisha", "Nour", "Layla", "Mariam",
                "Hana", "Salma", "Nadia", "Dina"
            };

            var lastNames = new[]
            {
                "Abdullah", "Rahman", "Hussein", "Karim", "Said", "Nasser", "Fahad", "Rashid",
                "Saleh", "Hamad"
            };

            for (int i = 1; i <= 50; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];

                members.Add(new Member
                {
                    Id = i,
                    MembershipNumber = $"MEM{DateTime.Now.Year}{(i + 1000)}",
                    FullName = $"{firstName} {lastName}",
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}@gmail.com",
                    Phone = $"+201{random.Next(10, 20)}{random.Next(10000000, 99999999)}",
                    Address = $"{random.Next(1, 500)} Main Street, Cairo, Egypt",
                    RegistrationDate = DateTime.Now.AddDays(-random.Next(0, 1000)),
                    IsActive = random.Next(100) > 10 // 90% active
                });
            }

            return members;
        }

        private static async Task<List<Borrowing>> GenerateBorrowings(ApplicationDbContext context)
        {
            var borrowings = new List<Borrowing>();
            var random = new Random();

            var books = await context.Books.ToListAsync();
            var members = await context.Members.ToListAsync();

            for (int i = 1; i <= 100; i++)
            {
                var book = books[random.Next(books.Count)];
                var member = members[random.Next(members.Count)];
                var borrowDate = DateTime.Now.AddDays(-random.Next(1, 365));
                var expectedReturnDate = borrowDate.AddDays(random.Next(7, 21));
                var isReturned = random.Next(100) > 30; // 70% returned

                DateTime? actualReturnDate = null;
                string status = "Borrowed";
                decimal lateFee = 0;

                if (isReturned)
                {
                    actualReturnDate = expectedReturnDate.AddDays(random.Next(-5, 15));
                    status = "Returned";

                    if (actualReturnDate > expectedReturnDate)
                    {
                        var daysLate = (actualReturnDate.Value - expectedReturnDate).Days;
                        lateFee = daysLate * 5;
                        status = "Late";
                    }
                }

                borrowings.Add(new Borrowing
                {
                    Id = i,
                    BookId = book.Id,
                    MemberId = member.Id,
                    BorrowDate = borrowDate,
                    ExpectedReturnDate = expectedReturnDate,
                    ActualReturnDate = actualReturnDate,
                    Status = status,
                    LateFee = lateFee
                });

                // Update available copies for active borrowings
                if (!isReturned && book.AvailableCopies > 0)
                {
                    book.AvailableCopies--;
                }
            }

            await context.SaveChangesAsync();
            return borrowings;
        }

        private static string GetRandomFirstName()
        {
            var names = new[] { "Ahmed", "Mohamed", "Ali", "Hassan", "Omar", "Sara", "Fatima", "Nour" };
            return names[new Random().Next(names.Length)];
        }

        private static string GetRandomLastName()
        {
            var names = new[] { "Abdullah", "Rahman", "Hussein", "Karim", "Said" };
            return names[new Random().Next(names.Length)];
        }

        private static string GetRandomStreet()
        {
            var streets = new[] { "Main St", "Park Ave", "Elm St", "Oak St", "Maple Ave", "Pine St" };
            return streets[new Random().Next(streets.Length)];
        }
    }
}