using Catalog.Domain.Entity;

namespace Catalog.Domain.Interfaces
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(string id);
        Task<List<Game>> GetAllAsync();
        Task<List<Game>> GetByUserIdAsync(string userId);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
        Task DeleteAsync(string id);
    }
}
