using InvoiceManagementPro.Data;
using InvoiceManagementPro.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceManagementPro.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Category.ToListAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Category.FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync(); // generate ID

                if (category.ImageUpload != null && category.ImageUpload.Length > 0)
                {
                    await ProcessImageUpload(category);
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Category.FindAsync(id);
            if (category == null) return NotFound();
            
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.Category.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id);
                    if (existingCategory != null)
                    {
                        category.ImagePath = existingCategory.ImagePath;
                    }

                    if (category.ImageUpload != null && category.ImageUpload.Length > 0)
                    {
                        await ProcessImageUpload(category);
                    }

                    category.UpdatedAt = DateTime.Now;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Category.FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories", id.ToString());
                if (Directory.Exists(uploadDir))
                {
                    Directory.Delete(uploadDir, true);
                }

                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task ProcessImageUpload(Category category)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "categories", category.CategoryId.ToString());
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(category.ImageUpload.FileName);
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await category.ImageUpload.CopyToAsync(stream);
            }
            
            category.ImagePath = $"/uploads/categories/{category.CategoryId}/{fileName}";
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.CategoryId == id);
        }
    }
}
