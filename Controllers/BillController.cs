using InvoiceManagementPro.Data;
using InvoiceManagementPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagementPro.Controllers
{
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bill/Index
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Bill
                .Include(b => b.Customer)
                .OrderByDescending(b => b.BillCreatedDate)
                .ToListAsync();
                
            return View(invoices);
        }

        // GET: Bill/Details/5 (Generated Bill View - Reference Mapped)
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Bill
                .Include(b => b.Customer)
                .Include(b => b.BillItems)
                    .ThenInclude(bi => bi.Product)
                .FirstOrDefaultAsync(m => m.BillId == id);

            if (invoice == null) return NotFound();

            var viewModel = new BillPrintViewModel
            {
                Invoice = invoice,
                Customer = invoice.Customer
            };

            // Generate Payment QR via QRCoder
            using (MemoryStream ms = new MemoryStream())
            {
                string qrData = $"upi://pay?pa=zettan@bank&pn=ZetranCorp&am={invoice.GrandTotalAmount}&cu=INR&tn=INV-{invoice.BillId}";
                QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    viewModel.QRCodeBase64 = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }

            return View(viewModel);
        }

        // GET: Bill/Create (Dynamic JS Form)
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customer
                .Select(c => new { c.CustomerId, Name = c.CustomerName + " (" + c.CustomerType + ")" })
                .ToListAsync();
                
            ViewBag.Products = await _context.Product
                .Select(p => new { p.ProductId, p.ProductName, p.ProductRate, p.ProductUnit })
                .ToListAsync();

            return View();
        }

        // POST: Bill/CreateInvoiceAjax (Accepts the complex nested JSON from JS Builder)
        [HttpPost]
        public async Task<IActionResult> CreateInvoiceAjax([FromBody] InvoiceSubmissionDto model)
        {
            if (model == null || !model.Items.Any())
            {
                return BadRequest("Invalid invoice data. Items are required.");
            }

            try
            {
                // 1. Build Header
                var bill = new Bill
                {
                    CustomerId = model.CustomerId,
                    BillCreatedDate = model.BillCreatedDate,
                    POBillNo = model.POBillNo,
                    EwayBillNo = model.EwayBillNo,
                    VehicleNo = model.VehicleNo,
                    ShippingCost = model.ShippingCost,
                    TermsAndConditions = model.TermsAndConditions,
                    Status = "Unpaid"
                };

                decimal rollingSubTotal = 0;
                decimal rollingDiscount = 0;
                decimal rollingTax = 0;

                // 2. Build Lines & Process Totals Server-Side for security
                foreach (var item in model.Items)
                {
                    var product = await _context.Product.FindAsync(item.ProductId);
                    if (product == null) continue;

                    decimal rate = Convert.ToDecimal(product.ProductRate);
                    decimal gross = rate * item.Quantity;
                    
                    decimal discountAmt = gross * (item.DiscountPercentage / 100m);
                    decimal taxableAmt = gross - discountAmt;
                    decimal taxAmt = taxableAmt * (item.TaxPercentage / 100m);
                    
                    decimal lineTotal = taxableAmt + taxAmt;

                    var billItem = new BillItem
                    {
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,
                        ItemRate = rate,
                        DiscountPercentage = item.DiscountPercentage,
                        TaxPercentage = item.TaxPercentage,
                        LineTotal = lineTotal
                    };

                    bill.BillItems.Add(billItem);

                    rollingSubTotal += gross;
                    rollingDiscount += discountAmt;
                    rollingTax += taxAmt;
                }

                if (!bill.BillItems.Any())
                {
                    return BadRequest("No valid products found for billing.");
                }

                // 3. Finalize Aggregates
                bill.SubTotal = rollingSubTotal;
                bill.TotalDiscount = rollingDiscount;
                bill.TotalTax = rollingTax;
                bill.GrandTotalAmount = rollingSubTotal - rollingDiscount + rollingTax + bill.ShippingCost;

                // Backwards compat legacy fields to prevent breaking existing views
                bill.TotalAmount = (int)bill.GrandTotalAmount;
                bill.BillAmount = (int)bill.SubTotal;

                _context.Bill.Add(bill);
                await _context.SaveChangesAsync();

                // 4. Return success and the new ID to redirect to Details
                return Json(new { success = true, invoiceId = bill.BillId, message = "Invoice generated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal error finalizing invoice: " + ex.Message);
            }
        }

        // GET: Bill/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Bill
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(m => m.BillId == id);
                
            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // POST: Bill/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Bill
                .Include(b => b.BillItems) // Cascade delete lines
                .FirstOrDefaultAsync(m => m.BillId == id);
                
            if (invoice != null)
            {
                _context.BillItem.RemoveRange(invoice.BillItems);
                _context.Bill.Remove(invoice);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Invoice deleted permanently!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
