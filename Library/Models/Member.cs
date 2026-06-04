using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Membership Number")]
        [StringLength(20)]
        public string MembershipNumber { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}