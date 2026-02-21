using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagementPro.Models
{
    public class BillItem
    {
        [Key]
        public int BillItemId { get; set; }

        // Link to Parent Invoice Header
        [Required]
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }

        // Link to Product
        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        // Line Item Details
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ItemRate { get; set; } // Snapshot of product price at time of billing

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountPercentage { get; set; } // e.g. 10.00
        
        [NotMapped]
        public decimal DiscountAmount => (ItemRate * Quantity) * (DiscountPercentage / 100);

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxPercentage { get; set; } // e.g. 18.00 (GST)
        
        [NotMapped]
        public decimal TaxAmount => ((ItemRate * Quantity) - DiscountAmount) * (TaxPercentage / 100);

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; } // (Qty * Rate) - Discount + Tax

        // Additional Ref Info
        public string? HSNCode { get; set; } // Tax classification code
    }
}
