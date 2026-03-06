using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
    {
        _logger.LogInformation("Recuperando todos os jogos");
        var games = await _gameService.GetAllAsync();
        _logger.LogInformation("Total de {Count} jogos recuperados", games.Count);
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetById(string id)
    {
        _logger.LogInformation("Recuperando jogo com Id {Id}", id);
        var game = await _gameService.GetByIdAsync(id);
        if (game is null)
        {
            _logger.LogWarning("Jogo não encontrado com Id {Id}", id);
            return NotFound();
        }
        _logger.LogInformation("Jogo {GameName} recuperado com sucesso", game.Name);
        return Ok(game);
    }

    [HttpGet("/api/ListGamesByUserId/{userId}")]
    public async Task<ActionResult<IEnumerable<UserGameGameDto>>> GetbyUserId(string userId)
    {
        _logger.LogInformation("Recuperando jogos do usuário {UserId}", userId);
        var games = await _gameService.GetByUserIdAsync(userId);
        _logger.LogInformation("Usuário {UserId} possui {Count} jogos", userId, games.Count());
        return Ok(games);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        _logger.LogInformation("Criando novo jogo: {GameName}", dto.Name);
        var game = await _gameService.CreateAsync(dto);
        _logger.LogInformation("Jogo criado com sucesso. Id: {GameId}", game.Id);
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateGameDto dto)
    {
        _logger.LogInformation("Atualizando jogo com Id {Id}: {GameName}", id, dto.Name);
        var updated = await _gameService.UpdateAsync(id, dto);
        if (!updated)
        {
            _logger.LogWarning("Jogo não encontrado para atualização com Id {Id}", id);
            return NotFound();
        }
        _logger.LogInformation("Jogo {Id} atualizado com sucesso", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogInformation("Deletando jogo com Id {Id}", id);
        var deleted = await _gameService.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Jogo não encontrado para deletar com Id {Id}", id);
            return NotFound();
        }
        _logger.LogInformation("Jogo {Id} deletado com sucesso", id);
        return NoContent();
    }
}
