using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Borrowing
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Book is required")]
        [ForeignKey("Book")]
        [Display(Name = "Book")]
        public int BookId { get; set; }

        // Remove [Required] from navigation property
        public virtual Book Book { get; set; }

        [Required(ErrorMessage = "Member is required")]
        [ForeignKey("Member")]
        [Display(Name = "Member")]
        public int MemberId { get; set; }

        // Remove [Required] from navigation property
        public virtual Member Member { get; set; }

        [Required(ErrorMessage = "Borrow date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Borrow Date")]
        public DateTime BorrowDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Expected return date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Expected Return Date")]
        public DateTime ExpectedReturnDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Actual Return Date")]
        public DateTime? ActualReturnDate { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Borrowed";

        [Display(Name = "Late Fee")]
        [DataType(DataType.Currency)]
        public decimal LateFee { get; set; }
    }
}