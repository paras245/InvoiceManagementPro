using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InvoiceManagementPro.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "Company / Name")]
        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        public string? Email { get; set; }

        [Display(Name = "Phone No")]
        [Required(ErrorMessage = "Phone No is required.")]
        public string PhoneNumber { get; set; }

        // Addressing 
        [Required(ErrorMessage = "City is required.")]
        public string CustomerCity { get; set; }

        [Display(Name = "Billing Address")]
        public string? BillingAddress { get; set; }

        [Display(Name = "Shipping Address")]
        public string? ShippingAddress { get; set; }

        public string? State { get; set; }
        public string? ZipCode { get; set; }

        // Professional Fields
        [Display(Name = "GSTIN / Tax ID")]
        public string? GSTIN { get; set; }

        [Display(Name = "Customer Type")]
        [Required(ErrorMessage = "Type is required.")]
        public string CustomerType { get; set; } // Business, Individual

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } // Active, Inactive

        // Visuals
        public string? LogoImagePath { get; set; }

        [NotMapped]
        public IFormFile? LogoImageUpload { get; set; }

        // Legacy/Notes
        [Display(Name = "Notes/Details")]
        public string? CustomerDetail { get; set; }

        [Display(Name = "Account Number")]
        public long? AccountNumber { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
