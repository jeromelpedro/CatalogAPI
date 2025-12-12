using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infra.Repositories;

public class UserGameRepository : IUserGameRepository
{
    private readonly AppDbContext _context;

    public UserGameRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserGame userGame)
    {
        _context.UserGames.Add(userGame);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string usuarioId, string gameId)
    {
        return await _context.UserGames.AnyAsync(ug =>
            ug.UserId == usuarioId &&
            ug.GameId == gameId);
    }
}
