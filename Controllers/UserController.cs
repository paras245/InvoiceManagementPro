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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var users = await _context.User.ToListAsync();
            return View(users);
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync(); // generate ID

                if (user.ProfileImageUpload != null && user.ProfileImageUpload.Length > 0)
                {
                    await ProcessImageUpload(user);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FindAsync(id);
            if (user == null) return NotFound();
            
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.User.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == id);
                    if (existingUser != null)
                    {
                        user.ProfileImagePath = existingUser.ProfileImagePath;
                    }

                    if (user.ProfileImageUpload != null && user.ProfileImageUpload.Length > 0)
                    {
                        await ProcessImageUpload(user);
                    }

                    user.UpdatedAt = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "User updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.User.FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users", id.ToString());
                if (Directory.Exists(uploadDir))
                {
                    Directory.Delete(uploadDir, true);
                }

                _context.User.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "User deleted successfully!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private async Task ProcessImageUpload(User user)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users", user.UserId.ToString());
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ProfileImageUpload.FileName);
            var filePath = Path.Combine(basePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await user.ProfileImageUpload.CopyToAsync(stream);
            }
            
            user.ProfileImagePath = $"/uploads/users/{user.UserId}/{fileName}";
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}
