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
        public IActionResult Index()
        {
            var allBills = _context.Bill.ToList();
            var obj = allBills.Select(bill =>
            {
                var newObj = new BillView();

                newObj.CustomerName = _context.Customer.Where(c => c.CustomerId == bill.CustomerId).Select(c => c.CustomerName).FirstOrDefault();
                newObj.ProductName = _context.Product.Where(p => p.ProductRate == bill.ProductQuantity).Select(p => p.ProductName).FirstOrDefault();

                newObj.BillId = bill.BillId;
                newObj.BillCreatedDate = bill.BillCreatedDate;
                newObj.ProductQuantity = bill.ProductQuantity;
                newObj.BillAmount = bill.BillAmount;
                newObj.TotalAmount = bill.TotalAmount;
                newObj.Quantity = bill.Quantity;
               // newObj.AccountNumber=

                return newObj;
            }).ToList();

            return View(obj);
        }

        // GET: Bill/Details/5
        public IActionResult Details(int id)
        {
            var bill = _context.Bill.Find(id);
            var newObj = new BillView();

            newObj.CustomerName = _context.Customer.Where(c => c.CustomerId == bill.CustomerId).Select(c => c.CustomerName).FirstOrDefault();
            newObj.ProductName = _context.Product.Where(p => p.ProductRate == bill.ProductQuantity).Select(p => p.ProductName).FirstOrDefault();

            newObj.BillId = bill.BillId;
            newObj.BillCreatedDate = bill.BillCreatedDate;
            newObj.ProductQuantity = bill.ProductQuantity;
            newObj.BillAmount = bill.BillAmount;
            newObj.TotalAmount = bill.TotalAmount;
            newObj.Quantity = bill.Quantity;

            return View(newObj);
        }

        // GET: Bill/Create
        public IActionResult Create()
        {
            PopulateDropdown();
            return View();
        }

        // POST: Bill/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Bill bill)
        {
            PopulateDropdown();
            if (ModelState.IsValid)
            {
                var newBill = new Bill();
                newBill.BillAmount = bill.ProductQuantity;
                newBill.CustomerId = bill.CustomerId;
                newBill.TotalAmount = bill.ProductQuantity * bill.Quantity;
                newBill.BillCreatedDate = bill.BillCreatedDate;
                newBill.ProductQuantity = bill.ProductQuantity;

                _context.Bill.Add(newBill);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(bill);
        }

        // GET: Bill/Edit/5
        public IActionResult Edit(int id)
        {
            var bill = _context.Bill.Find(id);
            return View(bill);
        }

        // POST: Bill/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Bill bill)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(bill).State = EntityState.Modified;
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(bill);
        }

        // GET: Bill/Delete/5
        public IActionResult Delete(int id)
        {
            var bill = _context.Bill.Find(id);
            var newObj = new BillView();
            newObj.CustomerName = _context.Customer.Where(c => c.CustomerId == bill.CustomerId).Select(c => c.CustomerName).FirstOrDefault();
            newObj.ProductName = _context.Product.Where(p => p.ProductRate == bill.ProductQuantity).Select(p => p.ProductName).FirstOrDefault();

            newObj.BillId = bill.BillId;
            newObj.BillCreatedDate = bill.BillCreatedDate;
            newObj.ProductQuantity = bill.ProductQuantity;
            newObj.BillAmount = bill.BillAmount;
            newObj.TotalAmount = bill.TotalAmount;
            newObj.Quantity = bill.Quantity;

            return View(newObj);
        }

        // POST: Bill/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var bill = _context.Bill.Find(id);
            _context.Bill.Remove(bill);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult PrintBill(int id)
        {
            var bill = _context.Bill.Find(id);
            var customer = _context.Customer.FirstOrDefault(c => c.CustomerId == bill.CustomerId);
            var product = _context.Product.FirstOrDefault(p => p.ProductRate == bill.ProductQuantity);

            var billView = new BillView
            {
                BillId = bill.BillId,
                CustomerName = customer?.CustomerName,
                TotalAmount = bill.TotalAmount,
                BillCreatedDate = bill.BillCreatedDate,
                ProductQuantity = bill.ProductQuantity,
                ProductName = product?.ProductName,
                ProductRate = product.ProductRate
            };

            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(billView.BillId.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    string base64Image = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    billView.QRCodeImage = base64Image;
                }
            }

            return View(billView);
        }


        // GET: Bill/GenerateQRCode/5
        public IActionResult GenerateQRCode(int id)
        {
            var bill = _context.Bill.Find(id);

            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrCodeGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrCodeGenerator.CreateQrCode(bill.ToString(), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
                {
                    qrCodeImage.Save(ms, ImageFormat.Png);
                    string base64Image = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                    return PartialView("_QRCodePartial", base64Image);
                }
            }
        }



        public void PopulateDropdown()
        {
            var productQuantity = _context.Product.Select(p =>
                new SelectListItem
                {
                    Value = p.ProductRate.ToString(),
                    Text = p.ProductName
                }).ToList();

            ViewData["ProductList"] = productQuantity;

            var customerList = _context.Customer.Select(c =>
                new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName
                }).ToList();

            ViewData["CustomerList"] = customerList;
        }
    }
}
