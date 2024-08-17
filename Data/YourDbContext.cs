using ExcelToSQLConverter.Models;
using Microsoft.EntityFrameworkCore;

namespace ExcelToSQLConverter.Data
{
    public class YourDbContext : DbContext
    {
        public YourDbContext(DbContextOptions<YourDbContext> options) : base(options)
        {
        }

        public DbSet<YourEntity> ? YourEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure your entities here if needed
        }
    }
}
