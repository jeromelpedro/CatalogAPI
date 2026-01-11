using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infra.Repositories;

public class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(string id)
    {
        return await _context.Games.FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Game>> GetAllAsync()
    {
        return await _context.Games.AsNoTracking().ToListAsync();
    }

    public async Task<List<Game>> GetByUserIdAsync(string userId)
    {
        return await (from ug in _context.UserGames
                      join g in _context.Games on ug.GameId equals g.Id
                      where ug.UserId == userId
                      select g).AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var game = await _context.Games.FirstOrDefaultAsync(x => x.Id == id);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }
}
