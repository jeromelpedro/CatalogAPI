using Catalog.Domain.Dto;

namespace Catalog.Application.Interfaces;

public interface IGameSearchService
{
    Task<IEnumerable<GameDto>> SearchAsync(string query);
    Task<int> ReindexAllAsync();
}
