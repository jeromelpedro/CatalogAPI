using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class GameReviewService : IGameReviewService
{
    private readonly IGameReviewRepository _gameReviewRepository;
    private readonly ILogger<GameReviewService> _logger;

    public GameReviewService(IGameReviewRepository gameReviewRepository, ILogger<GameReviewService> logger)
    {
        _gameReviewRepository = gameReviewRepository;
        _logger = logger;
    }

    public async Task<GameReviewDto> AddReviewAsync(string gameId, CreateGameReviewDto dto)
    {
        _logger.LogTrace("Iniciando AddReviewAsync em GameReviewService para GameId {GameId}", gameId);
        _logger.LogInformation("Criando avaliação para jogo {GameId} pelo usuário {UserId}", gameId, dto.UserId);

        if (dto.Rating < 1 || dto.Rating > 5)
        {
            _logger.LogWarning("Nota inválida em AddReviewAsync para GameId {GameId}. Rating: {Rating}", gameId, dto.Rating);
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "A nota deve estar entre 1 e 5.");
        }

        var review = new GameReview
        {
            GameId = gameId,
            UserId = dto.UserId,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _gameReviewRepository.AddAsync(review);
        _logger.LogInformation("Avaliação criada com sucesso. ReviewId: {ReviewId}, GameId: {GameId}", review.Id, gameId);
        _logger.LogTrace("Finalizando AddReviewAsync em GameReviewService para GameId {GameId}", gameId);

        return new GameReviewDto
        {
            Id = review.Id,
            GameId = review.GameId,
            UserId = review.UserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<IEnumerable<GameReviewDto>> GetReviewsByGameIdAsync(string gameId)
    {
        _logger.LogTrace("Iniciando GetReviewsByGameIdAsync em GameReviewService para GameId {GameId}", gameId);
        _logger.LogInformation("Buscando avaliações do jogo {GameId}", gameId);

        var reviews = await _gameReviewRepository.GetByGameIdAsync(gameId);
        var result = reviews.Select(r => new GameReviewDto
        {
            Id = r.Id,
            GameId = r.GameId,
            UserId = r.UserId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();

        _logger.LogInformation("Total de {Count} avaliações recuperadas para GameId {GameId}", result.Count, gameId);
        _logger.LogTrace("Finalizando GetReviewsByGameIdAsync em GameReviewService para GameId {GameId}", gameId);
        return result;
    }
}