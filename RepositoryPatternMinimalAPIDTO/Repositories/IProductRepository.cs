using RepositoryPatternMinimalAPIDTO.Models;

namespace RepositoryPatternMinimalAPIDTO.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetFilteredAsync(string? nameFilter, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<int> CountAsync(string? nameFilter, decimal? minPrice, decimal? maxPrice);
    }

}
