using InvoiceManagementPro.Data;
using InvoiceManagementPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;

namespace InvoiceManagementPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        // Ensure we inject the ApplicationDbContext
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Fetch metric counts
            ViewBag.TotalProducts = _context.Product.Count();
            ViewBag.TotalCustomers = _context.Customer.Count();
            ViewBag.TotalInvoices = _context.Bill.Count();
            
            // Calculate total revenue from all bills (sum of TotalAmount)
            ViewBag.TotalRevenue = _context.Bill.Sum(b => (decimal?)b.TotalAmount) ?? 0m;
            
            // Fetch recent 5 invoices
            ViewBag.RecentInvoices = _context.Bill
                .OrderByDescending(b => b.BillCreatedDate)
                .Take(5)
                .ToList();

            // Fetch recent 5 customers
            ViewBag.RecentCustomers = _context.Customer
                .OrderByDescending(c => c.CustomerId)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}