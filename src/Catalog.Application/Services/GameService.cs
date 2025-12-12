using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
//using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    //private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
        //_logger = logger; //, ILogger<GameService> logger
    }

    public async Task<List<GameDto>> GetAllAsync()
    {
        var games = await _gameRepository.GetAllAsync();

        return games.Select(g => new GameDto
        {
            Id = g.Id,
            Name = g.Name,
            Genre = g.Genre,
            Published = g.Published,
            Active = g.Active,
            Price = g.Price,
            PromotionalPrice = g.PromotionalPrice
        }).ToList();
    }

    public async Task<GameDto?> GetByIdAsync(string id)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game is null)
            return null;

        return new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Published = game.Published,
            Active = game.Active,
            Price = game.Price,
            PromotionalPrice = game.PromotionalPrice
        };
    }

    public async Task<GameDto> CreateAsync(CreateGameDto dto)
    {
        var game = new Game
        {
            Name = dto.Name,
            Genre = dto.Genre,
            Published = dto.Published,
            Active = dto.Active,
            Price = dto.Price,
            PromotionalPrice = dto.PromotionalPrice,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _gameRepository.AddAsync(game);

        //_logger.LogInformation("Jogo criado com Id {Id}", game.Id);

        return new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Published = game.Published,
            Active = game.Active,
            Price = game.Price,
            PromotionalPrice = game.PromotionalPrice
        };
    }

    public async Task<bool> UpdateAsync(string id, CreateGameDto dto)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game is null)
            return false;

        game.Name = dto.Name;
        game.Genre = dto.Genre;
        game.Published = dto.Published;
        game.Active = dto.Active;
        game.Price = dto.Price;
        game.PromotionalPrice = dto.PromotionalPrice;
        game.UpdatedAt = DateTime.UtcNow;

        await _gameRepository.UpdateAsync(game);

        //_logger.LogInformation("Jogo atualizado Id {Id}", game.Id);

        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var game = await _gameRepository.GetByIdAsync(id);

        if (game is null)
            return false;

        await _gameRepository.DeleteAsync(id);

        //_logger.LogInformation("Jogo removido Id {Id}", id);

        return true;
    }
}
