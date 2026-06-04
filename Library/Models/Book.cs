using System;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Book title is required")]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author name is required")]
        [StringLength(100)]
        [Display(Name = "Author")]
        public string Author { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string Category { get; set; }

        [Display(Name = "ISBN")]
        [StringLength(13)]
        public string ISBN { get; set; }

        [Range(1, 10000)]
        [Display(Name = "Total Copies")]
        public int Copies { get; set; }

        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Publish Date")]
        public DateTime? PublishDate { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}