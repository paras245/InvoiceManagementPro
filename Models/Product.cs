using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InvoiceManagementPro.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } // e.g., "Active", "Inactive"

        [Required(ErrorMessage = "Product unit is required.")]
        public string ProductUnit { get; set; }

        [Required(ErrorMessage = "Product rate is required.")]
        public int ProductRate { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        public string ProductDescription { get; set; }

        // Persistent database paths
        public string? ImagePath1 { get; set; }
        public string? ImagePath2 { get; set; }
        public string? ImagePath3 { get; set; }

        // Form handlers, not saved to DB schema directly
        [NotMapped]
        public IFormFile? ImageUpload1 { get; set; }
        
        [NotMapped]
        public IFormFile? ImageUpload2 { get; set; }
        
        [NotMapped]
        public IFormFile? ImageUpload3 { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now; 
    }
}
