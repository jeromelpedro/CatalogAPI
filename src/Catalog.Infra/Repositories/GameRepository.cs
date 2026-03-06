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
        _logger.LogDebug("Buscando jogo com Id {Id} no banco de dados", id);
        try
        {
            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
            if (game is null)
                _logger.LogDebug("Jogo não encontrado com Id {Id}", id);
            else
                _logger.LogDebug("Jogo encontrado: {GameName} (Id: {Id})", game.Name, id);
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
        _logger.LogDebug("Buscando todos os jogos no banco de dados");
        try
        {
            var games = await _context.Games.AsNoTracking().ToListAsync();
            _logger.LogDebug("Total de {Count} jogos encontrados", games.Count);
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
        _logger.LogDebug("Buscando jogos do usuário {UserId} no banco de dados", userId);
        try
        {
            var games = await (from ug in _context.UserGames
                          join g in _context.Games on ug.GameId equals g.Id
                          where ug.UserId == userId
                          select g).AsNoTracking().ToListAsync();
            _logger.LogDebug("Usuário {UserId} possui {Count} jogos", userId, games.Count);
            return games;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogos do usuário {UserId}", userId);
            throw;
        }
    }

    public async Task AddAsync(Game game)
    {
        _logger.LogInformation("Adicionando novo jogo no banco de dados. Nome: {GameName}, Id: {Id}", game.Name, game.Id);
        try
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
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
        _logger.LogInformation("Atualizando jogo no banco de dados. Id: {Id}, Nome: {GameName}", game.Id, game.Name);
        try
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
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
        _logger.LogInformation("Deletando jogo com Id {Id}", id);
        try
        {
            var game = await _context.Games.FirstOrDefaultAsync(x => x.Id == id);
            if (game != null)
            {
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();
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
