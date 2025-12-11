using Microsoft.EntityFrameworkCore;
using RepositoryPatternMinimalAPIDTO.Data;
using RepositoryPatternMinimalAPIDTO.Models;
namespace RepositoryPatternMinimalAPIDTO.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(AppDbContext db, ILogger<ProductRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Product> AddAsync(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Product added with id {Id}", product.Id);
            return product;
        }

        public async Task<int> CountAsync(string? nameFilter, decimal? minPrice, decimal? maxPrice)
        {
            var query = ApplyFilter(_db.Products.AsQueryable(), nameFilter, minPrice, maxPrice);
            return await query.CountAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return;
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Product deleted with id {Id}", id);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Product>> GetFilteredAsync(string? nameFilter, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
        {
            var query = ApplyFilter(_db.Products.AsQueryable(), nameFilter, minPrice, maxPrice)
                        .AsNoTracking()
                        .OrderBy(p => p.Id)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize);

            return await query.ToListAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Product updated with id {Id}", product.Id);
        }

        // helper to apply filtering
        private static IQueryable<Product> ApplyFilter(IQueryable<Product> query, string? nameFilter, decimal? minPrice, decimal? maxPrice)
        {
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                query = query.Where(p => p.Name.Contains(nameFilter));
            }
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }
            return query;
        }
    }

}
