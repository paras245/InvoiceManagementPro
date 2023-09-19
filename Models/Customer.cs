using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementPro.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Name is required.")]
        public string CustomerName { get; set; }

        public string CustomerDetail { get; set; }

        [Display(Name = "City")]
        [Required(ErrorMessage = "City is required.")]
        public string CustomerCity { get; set; }

        [Display(Name = "Phone No")]
        [Required(ErrorMessage = "Phone No is required.")]
        public string PhoneNumber { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public long AccountNumber { get; set; }
    }
}
