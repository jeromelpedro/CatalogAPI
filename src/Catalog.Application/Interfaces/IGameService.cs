using Catalog.Domain.Dto;

namespace Catalog.Application.Interfaces;

public interface IGameService
{
    Task<List<GameDto>> GetAllAsync();
    Task<GameDto?> GetByIdAsync(string id);
    Task<IEnumerable<UserGameGameDto>> GetByUserIdAsync(string userId);
    Task<IEnumerable<TopGameDto>> GetTopGamesAsync();
    Task<GameDto> CreateAsync(CreateGameDto dto);
    Task<bool> UpdateAsync(string id, CreateGameDto dto);
    Task<bool> DeleteAsync(string id);
}
