using System;
using System.Collections.Generic;

namespace InvoiceManagementPro.Models
{
    // DTO for accepting AJAX Invoice submissions from the JS UI
    public class InvoiceSubmissionDto
    {
        public int CustomerId { get; set; }
        public DateTime BillCreatedDate { get; set; }
        public string? POBillNo { get; set; }
        public string? EwayBillNo { get; set; }
        public string? VehicleNo { get; set; }
        public decimal ShippingCost { get; set; }
        public string? TermsAndConditions { get; set; }
        
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }

    public class InvoiceItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxPercentage { get; set; }
    }

    // View Model for rendering printable bills (if needed over raw Entity)
    public class BillPrintViewModel
    {
        public Bill Invoice { get; set; }
        public Customer Customer { get; set; }
        public string QRCodeBase64 { get; set; }
    }
}
