using InvoiceManagementPro.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManagementPro.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions <ApplicationDbContext> options):base(options) 
        {
                
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Bill> Bill { get; set; }
        public DbSet<BillItem> BillItem { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<User> User { get; set; }
    }
}
