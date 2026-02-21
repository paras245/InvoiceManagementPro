using ClosedXML.Excel;
using InvoiceManagementPro.Data;
using InvoiceManagementPro.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagementPro.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        private IConfiguration _configuration;

        public ProductController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: ProductController
        public async Task<ActionResult> Index()
        {
            var products = await _context.Product.ToListAsync();
            return View(products);
        }

        // GET: ProductController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: ProductController/Create
        public async Task<ActionResult> Create()
        {
            ViewBag.Categories = await _context.Category
                .Where(c => c.Status == "Active")
                .Select(c => c.CategoryName)
                .ToListAsync();
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Product.Add(product);
                await _context.SaveChangesAsync(); // Save to get the generated ProductId

                // Handle Image Uploads
                await ProcessImageUploads(product);

                // Save again if images were added
                if(product.ImagePath1 != null || product.ImagePath2 != null || product.ImagePath3 != null)
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Categories = await _context.Category.Where(c => c.Status == "Active").Select(c => c.CategoryName).ToListAsync();
            return View(product);
        }

        // GET: ProductController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = await _context.Category
                .Where(c => c.Status == "Active")
                .Select(c => c.CategoryName)
                .ToListAsync();
            return View(product);
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch existing to preserve paths if no new image is uploaded
                    var existingProduct = await _context.Product.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    if(existingProduct != null)
                    {
                        product.ImagePath1 = existingProduct.ImagePath1;
                        product.ImagePath2 = existingProduct.ImagePath2;
                        product.ImagePath3 = existingProduct.ImagePath3;
                    }

                    // Process potential new uploads (will overwrite paths if new files are provided)
                    await ProcessImageUploads(product);

                    _context.Entry(product).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Product updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Categories = await _context.Category.Where(c => c.Status == "Active").Select(c => c.CategoryName).ToListAsync();
            return View(product);
        }

        // GET: ProductController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Remove product directory if it exists
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", id.ToString());
            if (Directory.Exists(uploadDir))
            {
                Directory.Delete(uploadDir, true);
            }

            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task ProcessImageUploads(Product product)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products", product.ProductId.ToString());

            if (product.ImageUpload1 != null && product.ImageUpload1.Length > 0)
            {
                product.ImagePath1 = await SaveImageAsync(product.ImageUpload1, basePath);
            }
            
            if (product.ImageUpload2 != null && product.ImageUpload2.Length > 0)
            {
                product.ImagePath2 = await SaveImageAsync(product.ImageUpload2, basePath);
            }

            if (product.ImageUpload3 != null && product.ImageUpload3.Length > 0)
            {
                product.ImagePath3 = await SaveImageAsync(product.ImageUpload3, basePath);
            }
        }

        private async Task<string> SaveImageAsync(IFormFile file, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(directoryPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Return relative path for web
            var folderName = new DirectoryInfo(directoryPath).Name;
            return $"/uploads/products/{folderName}/{fileName}";
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(p => p.ProductId == id);
        }

        [HttpPost]
        public IActionResult Export()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                DataTable dt = this.GetProducts().Tables[0];
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    string FileName = DateTime.Now + "Product.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", FileName);
                }
            }
        }

        private DataSet GetProducts()
        {
            DataSet ds = new DataSet();

            string constr = _configuration.GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
            
            using (SqlConnection con = new SqlConnection(constr))
            {
                string query = "SELECT * FROM Product";
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
