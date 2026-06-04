# Library Management System

A simple ASP.NET Core MVC library management system built with .NET 8, Entity Framework Core and ASP.NET Core Identity. The project includes role-based authentication, user profiles, book catalog, member management and borrowing tracking with sample data seeding.

## Features

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core with SQL Server
- ASP.NET Core Identity with custom `ApplicationUser`
- Role-based access: Admin, Librarian, User
- Database seeding (roles, admin, librarian, sample users, books, members, borrowings)
- CRUD for Books, Members and Borrowings
- Seeded sample data for development and testing

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 (or newer) with ASP.NET and web development workload
- SQL Server instance (LocalDB or full SQL Server)
  

## Project structure (important folders)

- `Controllers` — MVC controllers (`BooksController`, `MembersController`, `BorrowingsController`, `AccountController`, `HomeController`)
- `Models` — Domain and Identity models (`ApplicationUser`, `Book`, `Member`, `Borrowing`)
- `Data` — `SeedData` and seeding logic
- `Context` — `ApplicationDbContext` EF Core DbContext
- `Views` — Razor views
- `wwwroot` — Static assets (CSS, JS, images)
- `Migrations` — EF Core migrations

## Identity & Authentication

- Identity is configured in `Program.cs` and uses the custom `ApplicationUser`.
- Roles are seeded in `ApplicationDbContext.OnModelCreating` and also ensured by `SeedData`.
- Cookie settings and password policies are configured in `Program.cs`.
