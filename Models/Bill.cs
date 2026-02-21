using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceManagementPro.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }

        // Header Information
        public string InvoiceNumber { get; set; } = $"INV-{DateTime.Now.Ticks.ToString().Substring(10, 5)}";
        
        [DataType(DataType.Date)]
        public DateTime BillCreatedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        // Additional Invoice Details (From Reference Image)
        public string? POBillNo { get; set; }
        public string? EwayBillNo { get; set; }
        public string? VehicleNo { get; set; }

        // Aggregates / Totals
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDiscount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotalAmount { get; set; }

        // Meta/State
        public string Status { get; set; } = "Unpaid"; // Unpaid, Paid, Cancelled
        public string? TermsAndConditions { get; set; }
        public string? InternalNotes { get; set; }

        // Relational Line Items
        public virtual ICollection<BillItem> BillItems { get; set; } = new List<BillItem>();

        // Legacy compat (Will try to deprecate these in logic, keeping to avoid breaking other areas severely without EF migration first, or mapping them)
        public int Quantity { get; set; } 
        public int ProductQuantity { get; set; }
        public int BillAmount { get; set; }
        public int TotalAmount { get; set; }
    }
}
