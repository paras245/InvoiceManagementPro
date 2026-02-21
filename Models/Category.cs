using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InvoiceManagementPro.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(100)]
        public string CategoryName { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } // Active, Inactive

        // Thumbnail Image
        public string? ImagePath { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }

        // Meta
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
