using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<List<GameDto>> GetAllAsync()
    {
        _logger.LogInformation("Iniciando busca de todos os jogos");
        try
        {
            var games = await _gameRepository.GetAllAsync();

            var result = games.Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Genre = g.Genre,
                Published = g.Published,
                Active = g.Active,
                Price = g.Price,
                PromotionalPrice = g.PromotionalPrice
            }).ToList();
            
            _logger.LogInformation("Busca de jogos concluída. Total: {Count} jogos retornados", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os jogos");
            throw;
        }
    }

    public async Task<GameDto?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Buscando jogo com Id {Id}", id);
        try
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game is null)
            {
                _logger.LogWarning("Jogo não encontrado com Id {Id}", id);
                return null;
            }

            var result = new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Published = game.Published,
                Active = game.Active,
                Price = game.Price,
                PromotionalPrice = game.PromotionalPrice
            };
            
            _logger.LogDebug("Jogo encontrado: {GameName} (Id: {Id})", game.Name, id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogo com Id {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<UserGameGameDto>> GetByUserIdAsync(string userId)
    {
        _logger.LogInformation("Buscando jogos do usuário {UserId}", userId);
        try
        {
            var games = await _gameRepository.GetByUserIdAsync(userId);

            var result = games.Select(g => new UserGameGameDto
            {
                Id = g.Id,
                Name = g.Name,
                Genre = g.Genre
            }).ToList();
            
            _logger.LogInformation("Usuário {UserId} possui {Count} jogos", userId, result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogos do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TopGameDto>> GetTopGamesAsync()
    {
        var games = await _gameRepository.GetTopGamesAsync();

        return games.Select(g => new TopGameDto
        {
            Id = g.Id,
            Name = g.Name,
            Genre = g.Genre,
            TotalPurchases = g.TotalPurchases
        }).ToList();
    }

    public async Task<GameDto> CreateAsync(CreateGameDto dto)
    {
        _logger.LogInformation("Iniciando criação de novo jogo: {GameName} - Gênero: {Genre}", dto.Name, dto.Genre);
        try
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

            _logger.LogInformation("Jogo criado com sucesso. Id: {Id}, Nome: {Name}", game.Id, game.Name);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar jogo: {GameName}", dto.Name);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(string id, CreateGameDto dto)
    {
        _logger.LogInformation("Atualizando jogo com Id {Id}: {GameName}", id, dto.Name);
        try
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game is null)
            {
                _logger.LogWarning("Jogo não encontrado para atualização. Id: {Id}", id);
                return false;
            }

            game.Name = dto.Name;
            game.Genre = dto.Genre;
            game.Published = dto.Published;
            game.Active = dto.Active;
            game.Price = dto.Price;
            game.PromotionalPrice = dto.PromotionalPrice;
            game.UpdatedAt = DateTime.UtcNow;

            await _gameRepository.UpdateAsync(game);

            _logger.LogInformation("Jogo atualizado com sucesso. Id: {Id}, Nome: {Name}", game.Id, game.Name);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar jogo com Id {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        _logger.LogInformation("Deletando jogo com Id {Id}", id);
        try
        {
            var game = await _gameRepository.GetByIdAsync(id);

            if (game is null)
            {
                _logger.LogWarning("Jogo não encontrado para deleção. Id: {Id}", id);
                return false;
            }

            await _gameRepository.DeleteAsync(id);

            _logger.LogInformation("Jogo deletado com sucesso. Id: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar jogo com Id {Id}", id);
            throw;
        }
    }
}
