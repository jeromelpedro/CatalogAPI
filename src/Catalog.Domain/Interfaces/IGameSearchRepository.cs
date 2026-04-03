using Catalog.Domain.Dto;
using Catalog.Domain.Entity;

namespace Catalog.Domain.Interfaces;

public interface IGameSearchRepository
{
    Task IndexAsync(Game game);
    Task<IEnumerable<GameDto>> SearchAsync(string query);
}
