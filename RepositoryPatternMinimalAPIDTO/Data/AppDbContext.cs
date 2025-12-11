using Microsoft.EntityFrameworkCore;
namespace RepositoryPatternMinimalAPIDTO.Data
{
    
    using RepositoryPatternMinimalAPIDTO.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().Property(p => p.Name).HasMaxLength(200).IsRequired();
        }
    }

}
