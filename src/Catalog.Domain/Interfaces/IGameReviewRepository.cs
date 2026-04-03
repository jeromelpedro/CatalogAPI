using Catalog.Domain.Entity;

namespace Catalog.Domain.Interfaces;

public interface IGameReviewRepository
{
    Task AddAsync(GameReview review);
    Task<List<GameReview>> GetByGameIdAsync(string gameId);
}