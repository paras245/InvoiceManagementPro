namespace InvoiceManagementPro.Models
{
    public class BillView
    {
        public int? BillId { get; set; }
        public int? BillAmount { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNumber { get; set; }
        public int? TotalAmount { get; set; }
        public DateTime? BillCreatedDate { get; set; }
        public int? ProductQuantity { get; set; }
        public string ProductName { get; set; }
        public int? Quantity { get; set; }
        public string QRCodeImage { get; set; } // Add this property for QR code image URL

        //For Bill
        public int ProductRate { get; set; }



        //For AccountNumber
        public int AccountNumber { get; set; }
    }
}
