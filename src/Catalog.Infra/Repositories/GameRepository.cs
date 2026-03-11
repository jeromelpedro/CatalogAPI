using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<GameRepository> _logger;

    public GameRepository(AppDbContext context, ILogger<GameRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Game?> GetByIdAsync(string id)
    {
        _logger.LogTrace("Iniciando consulta GetByIdAsync em GameRepository para Id {Id}", id);
        _logger.LogDebug("Buscando jogo com Id {Id} no banco de dados", id);
        try
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
            if (game is null)
                _logger.LogDebug("Jogo não encontrado com Id {Id}", id);
            else
                _logger.LogDebug("Jogo encontrado: {GameName} (Id: {Id})", game.Name, id);
            _logger.LogTrace("Consulta GetByIdAsync finalizada em GameRepository para Id {Id}", id);
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogo com Id {Id}", id);
            throw;
        }
    }

    public async Task<List<Game>> GetAllAsync()
    {
        _logger.LogTrace("Iniciando consulta GetAllAsync em GameRepository");
        _logger.LogDebug("Buscando todos os jogos no banco de dados");
        try
        {
            var games = await _context.Games.AsNoTracking().ToListAsync();
            _logger.LogDebug("Total de {Count} jogos encontrados", games.Count);
            _logger.LogTrace("Consulta GetAllAsync finalizada em GameRepository");
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os jogos");
            throw;
        }
    }

    public async Task<List<Game>> GetByUserIdAsync(string userId)
    {
        _logger.LogTrace("Iniciando consulta GetByUserIdAsync em GameRepository para UserId {UserId}", userId);
        _logger.LogDebug("Buscando jogos do usuário {UserId} no banco de dados", userId);
        try
        {
            var games = await (from ug in _context.UserGames
                          join g in _context.Games on ug.GameId equals g.Id
                          where ug.UserId == userId
                          select g).AsNoTracking().ToListAsync();
            _logger.LogDebug("Usuário {UserId} possui {Count} jogos", userId, games.Count);
            _logger.LogTrace("Consulta GetByUserIdAsync finalizada em GameRepository para UserId {UserId}", userId);
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogos do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task<List<TopGameProjection>> GetTopGamesAsync()
    {
        _logger.LogTrace("Iniciando consulta GetTopGamesAsync em GameRepository");
        var result = await _context.UserGames
            .GroupBy(ug => ug.GameId)
            .Select(group => new
            {
                GameId = group.Key,
                TotalPurchases = group.Count()
            })
            .OrderByDescending(x => x.TotalPurchases)
            .Take(3)
            .Join(_context.Games,
                  grouped => grouped.GameId,
                  game => game.Id,
                  (grouped, game) => new TopGameProjection
                  {
                      Id = game.Id,
                      Name = game.Name,
                      Genre = game.Genre,
                      TotalPurchases = grouped.TotalPurchases
                  })
            .AsNoTracking()
            .ToListAsync();

            _logger.LogTrace("Consulta GetTopGamesAsync finalizada em GameRepository com {Count} registros", result.Count);
            return result;
    }

    public async Task AddAsync(Game game)
    {
        _logger.LogTrace("Iniciando AddAsync em GameRepository para jogo {GameName}", game.Name);
        _logger.LogInformation("Adicionando novo jogo no banco de dados. Nome: {GameName}, Id: {Id}", game.Name, game.Id);
        try
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            _logger.LogTrace("AddAsync finalizado em GameRepository para Id {Id}", game.Id);
            _logger.LogInformation("Jogo adicionado com sucesso. Id: {Id}", game.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jogo. Nome: {GameName}", game.Name);
            throw;
        }
    }

    public async Task UpdateAsync(Game game)
    {
        _logger.LogTrace("Iniciando UpdateAsync em GameRepository para Id {Id}", game.Id);
        _logger.LogInformation("Atualizando jogo no banco de dados. Id: {Id}, Nome: {GameName}", game.Id, game.Name);
        try
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            _logger.LogTrace("UpdateAsync finalizado em GameRepository para Id {Id}", game.Id);
            _logger.LogInformation("Jogo atualizado com sucesso. Id: {Id}", game.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar jogo com Id {Id}", game.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogTrace("Iniciando DeleteAsync em GameRepository para Id {Id}", id);
        _logger.LogInformation("Deletando jogo com Id {Id}", id);
        try
        {
            var game = await _context.Games.FirstOrDefaultAsync(x => x.Id == id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
                _logger.LogTrace("DeleteAsync finalizado em GameRepository para Id {Id}", id);
                _logger.LogInformation("Jogo deletado com sucesso. Id: {Id}", id);
            }
            else
            {
                _logger.LogWarning("Jogo não encontrado para deleção. Id: {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar jogo com Id {Id}", id);
            throw;
        }
    }
}
