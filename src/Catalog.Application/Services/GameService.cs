using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameSearchRepository _searchRepository;
    private readonly ILogger<GameService> _logger;

    public GameService(IGameRepository gameRepository, IGameSearchRepository searchRepository, ILogger<GameService> logger)
    {
        _gameRepository = gameRepository;
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public async Task<List<GameDto>> GetAllAsync()
    {
        _logger.LogTrace("Iniciando GetAllAsync em GameService");
        _logger.LogInformation("Iniciando busca de todos os jogos");
        try
        {
            var games = await _gameRepository.GetAllAsync();
            _logger.LogTrace("Mapeando {Count} jogos para DTO em GameService.GetAllAsync", games.Count);

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
            _logger.LogTrace("Finalizando GetAllAsync em GameService");
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
        _logger.LogTrace("Iniciando GetByIdAsync em GameService para Id {Id}", id);
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
            _logger.LogTrace("Finalizando GetByIdAsync em GameService para Id {Id}", id);
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
        _logger.LogTrace("Iniciando GetByUserIdAsync em GameService para UserId {UserId}", userId);
        _logger.LogInformation("Buscando jogos do usuário {UserId}", userId);
        try
        {
            var games = await _gameRepository.GetByUserIdAsync(userId);
            _logger.LogTrace("Mapeando {Count} jogos para UserGameGameDto em GameService.GetByUserIdAsync", games.Count);

            var result = games.Select(g => new UserGameGameDto
            {
                Id = g.Id,
                Name = g.Name,
                Genre = g.Genre
            }).ToList();
            
            _logger.LogInformation("Usuário {UserId} possui {Count} jogos", userId, result.Count);
            _logger.LogTrace("Finalizando GetByUserIdAsync em GameService para UserId {UserId}", userId);
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
        _logger.LogTrace("Iniciando GetTopGamesAsync em GameService");
        var games = await _gameRepository.GetTopGamesAsync();
        _logger.LogTrace("Mapeando {Count} jogos do ranking em GameService.GetTopGamesAsync", games.Count);

        var result = games.Select(g => new TopGameDto
        {
            Id = g.Id,
            Name = g.Name,
            Genre = g.Genre,
            TotalPurchases = g.TotalPurchases
        }).ToList();

        _logger.LogTrace("Finalizando GetTopGamesAsync em GameService");
        return result;
    }

    public async Task<GameDto> CreateAsync(CreateGameDto dto)
    {
        _logger.LogTrace("Iniciando CreateAsync em GameService para jogo {GameName}", dto.Name);
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
            _logger.LogTrace("Persistência concluída em GameService.CreateAsync para Id {Id}", game.Id);

            await IndexSilentlyAsync(game);

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
        _logger.LogTrace("Iniciando UpdateAsync em GameService para Id {Id}", id);
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
            _logger.LogTrace("Persistência concluída em GameService.UpdateAsync para Id {Id}", id);

            await IndexSilentlyAsync(game);

            _logger.LogInformation("Jogo atualizado com sucesso. Id: {Id}, Nome: {Name}", game.Id, game.Name);
            _logger.LogTrace("Finalizando UpdateAsync em GameService para Id {Id}", id);

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
        _logger.LogTrace("Iniciando DeleteAsync em GameService para Id {Id}", id);
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
            _logger.LogTrace("Persistência concluída em GameService.DeleteAsync para Id {Id}", id);

            _logger.LogInformation("Jogo deletado com sucesso. Id: {Id}", id);
            _logger.LogTrace("Finalizando DeleteAsync em GameService para Id {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar jogo com Id {Id}", id);
            throw;
        }
    }

    private async Task IndexSilentlyAsync(Game game)
    {
        try
        {
            await _searchRepository.IndexAsync(game);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao indexar jogo Id {Id} no Elasticsearch — operação principal não foi afetada", game.Id);
        }
    }
}
