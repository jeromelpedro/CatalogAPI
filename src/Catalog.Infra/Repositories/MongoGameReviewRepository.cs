using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Catalog.Infra.Repositories;

public class MongoGameReviewRepository : IGameReviewRepository
{
    private readonly IMongoCollection<GameReview> _reviews;
    private readonly ILogger<MongoGameReviewRepository> _logger;

    public MongoGameReviewRepository(IMongoDatabase database, IConfiguration configuration, ILogger<MongoGameReviewRepository> logger)
    {
        _logger = logger;

        var collectionName = configuration["MongoDb:ReviewCollection"] ?? "game_reviews";
        _reviews = database.GetCollection<GameReview>(collectionName);

        var gameIdIndex = new CreateIndexModel<GameReview>(Builders<GameReview>.IndexKeys.Ascending(x => x.GameId));
        var createdAtIndex = new CreateIndexModel<GameReview>(Builders<GameReview>.IndexKeys.Descending(x => x.CreatedAt));
        _reviews.Indexes.CreateMany(new[] { gameIdIndex, createdAtIndex });
    }

    public async Task AddAsync(GameReview review)
    {
        _logger.LogTrace("Iniciando AddAsync em MongoGameReviewRepository para GameId {GameId}", review.GameId);

        if (string.IsNullOrWhiteSpace(review.Id))
            review.Id = Guid.NewGuid().ToString("N");

        await _reviews.InsertOneAsync(review);
        _logger.LogTrace("Finalizando AddAsync em MongoGameReviewRepository para ReviewId {ReviewId}", review.Id);
    }

    public async Task<List<GameReview>> GetByGameIdAsync(string gameId)
    {
        _logger.LogTrace("Iniciando GetByGameIdAsync em MongoGameReviewRepository para GameId {GameId}", gameId);

        var reviews = await _reviews
            .Find(r => r.GameId == gameId)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();

        _logger.LogTrace("Finalizando GetByGameIdAsync em MongoGameReviewRepository para GameId {GameId} com {Count} itens", gameId, reviews.Count);
        return reviews;
    }
}