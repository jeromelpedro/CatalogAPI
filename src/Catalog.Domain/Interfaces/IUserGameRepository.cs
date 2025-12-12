using Catalog.Domain.Entity;

namespace Catalog.Domain.Interfaces
{
    public interface IUserGameRepository
    {
        Task AddAsync(UserGame userGame);
        Task<bool> ExistsAsync(string usuarioId, string gameId);
    }
}
