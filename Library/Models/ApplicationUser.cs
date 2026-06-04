using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add custom properties
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? Address { get; set; }
        public string PhoneNumber { get; set; }
    }
}