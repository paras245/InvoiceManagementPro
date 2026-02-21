using ClosedXML.Excel;
using InvoiceManagementPro.Data;
using InvoiceManagementPro.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagementPro.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        private IConfiguration _configuration;

        public CustomerController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customer.ToListAsync();
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = System.DateTime.Now;
                customer.UpdatedAt = System.DateTime.Now;
                
                _context.Customer.Add(customer);
                await _context.SaveChangesAsync(); // generate ID

                if (customer.LogoImageUpload != null && customer.LogoImageUpload.Length > 0)
                {
                    await ProcessImageUpload(customer);
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customer.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == id);
                    if (existingCustomer != null)
                    {
                        customer.LogoImagePath = existingCustomer.LogoImagePath;
                        customer.CreatedAt = existingCustomer.CreatedAt;
                    }

                    if (customer.LogoImageUpload != null && customer.LogoImageUpload.Length > 0)
                    {
                        await ProcessImageUpload(customer);
                    }

                    customer.UpdatedAt = System.DateTime.Now;
                    
                    _context.Customer.Update(customer);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // GET: Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.CustomerId == id);
            
            if (customer != null)
            {
                // Delete associated images
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", id.ToString());
                if (Directory.Exists(uploadDir))
                {
                    Directory.Delete(uploadDir, true);
                }

                _context.Customer.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task ProcessImageUpload(Customer customer)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", customer.CustomerId.ToString());
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = System.Guid.NewGuid().ToString() + Path.GetExtension(customer.LogoImageUpload.FileName);
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await customer.LogoImageUpload.CopyToAsync(stream);
            }
            
            customer.LogoImagePath = $"/uploads/customers/{customer.CustomerId}/{fileName}";
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(c => c.CustomerId == id);
        }

        [HttpPost]
        public IActionResult Export()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                DataTable dt = this.GetCustomer().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    string FileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + "Customers.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
                }
            }
        }

        private DataSet GetCustomer()
        {
            DataSet ds = new DataSet();
            string constr = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT * FROM Customer";
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(ds);
                    }
                }
            }
            return ds;
        }
    }
}
