using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementPro.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product unit is required.")]
        public string ProductUnit { get; set; }

        [Required(ErrorMessage = "Product rate is required.")]
        public int ProductRate { get; set; }

        [Required(ErrorMessage = "Product description is required.")]
        public string ProductDescription { get; set; }


        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.Now; 
    }
}
