using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace InvoiceManagementPro.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        // Personal Information
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50)]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; } // Male, Female, Other

        // Employment Information
        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } // Admin, Manager, Staff

        [Required(ErrorMessage = "Employment type is required.")]
        public string EmploymentType { get; set; } // Regular, Contract, Freelance

        [Required(ErrorMessage = "Date of joining is required.")]
        [DataType(DataType.Date)]
        public DateTime DateOfJoining { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } // Active, Inactive, Suspended

        // Profile Image
        public string? ProfileImagePath { get; set; }

        [NotMapped]
        public IFormFile? ProfileImageUpload { get; set; }

        // Meta
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
    }
}
