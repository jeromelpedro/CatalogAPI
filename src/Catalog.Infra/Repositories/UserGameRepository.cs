using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.Repositories;

public class UserGameRepository : IUserGameRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserGameRepository> _logger;

    public UserGameRepository(AppDbContext context, ILogger<UserGameRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(UserGame userGame)
    {
        _logger.LogTrace("Iniciando AddAsync em UserGameRepository para UserId {UserId}, GameId {GameId}", userGame.UserId, userGame.GameId);
        _logger.LogInformation("Adicionando jogo à biblioteca do usuário. UserId: {UserId}, GameId: {GameId}", userGame.UserId, userGame.GameId);
        try
        {
            _context.UserGames.Add(userGame);
            await _context.SaveChangesAsync();
            _logger.LogTrace("AddAsync finalizado em UserGameRepository para UserId {UserId}, GameId {GameId}", userGame.UserId, userGame.GameId);
            _logger.LogInformation("Jogo adicionado com sucesso à biblioteca do usuário {UserId}", userGame.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jogo à biblioteca. UserId: {UserId}, GameId: {GameId}", userGame.UserId, userGame.GameId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string usuarioId, string gameId)
    {
        _logger.LogTrace("Iniciando ExistsAsync em UserGameRepository para UserId {UserId}, GameId {GameId}", usuarioId, gameId);
        _logger.LogDebug("Verificando se jogo {GameId} existe na biblioteca do usuário {UserId}", gameId, usuarioId);
        try
        {
            var exists = await _context.UserGames.AnyAsync(ug =>
                ug.UserId == usuarioId &&
                ug.GameId == gameId);
            
            if (exists)
                _logger.LogDebug("Jogo {GameId} encontrado na biblioteca do usuário {UserId}", gameId, usuarioId);
            else
                _logger.LogDebug("Jogo {GameId} não encontrado na biblioteca do usuário {UserId}", gameId, usuarioId);

            _logger.LogTrace("ExistsAsync finalizado em UserGameRepository para UserId {UserId}, GameId {GameId}", usuarioId, gameId);
            
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do jogo. UserId: {UserId}, GameId: {GameId}", usuarioId, gameId);
            throw;
        }
    }
}
