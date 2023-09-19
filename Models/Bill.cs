using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementPro.Models
{
    public class Bill
    {
        [Key]
        public int BillId { get; set; }
        public int BillAmount { get; set; }
        public int CustomerId { get; set; }
        public int TotalAmount { get; set; }
        public DateTime BillCreatedDate { get; set; }
        public int ProductQuantity { get; set; }

        public int Quantity { get; set; }


    }
}
