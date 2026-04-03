using Catalog.Domain.Dto;

namespace Catalog.Application.Interfaces;

public interface IGameReviewService
{
    Task<GameReviewDto> AddReviewAsync(string gameId, CreateGameReviewDto dto);
    Task<IEnumerable<GameReviewDto>> GetReviewsByGameIdAsync(string gameId);
}