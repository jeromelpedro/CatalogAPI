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
    private readonly IGameSearchService _gameSearchService;
    private readonly IGameReviewService _gameReviewService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, IGameSearchService gameSearchService, IGameReviewService gameReviewService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _gameSearchService = gameSearchService;
        _gameReviewService = gameReviewService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
    {
        _logger.LogTrace("Iniciando fluxo GetAll em GamesController");
        _logger.LogInformation("Recuperando todos os jogos");
        var games = await _gameService.GetAllAsync();
        _logger.LogTrace("Fluxo GetAll finalizado em GamesController");
        _logger.LogInformation("Total de {Count} jogos recuperados", games.Count);
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetById(string id)
    {
        _logger.LogTrace("Iniciando fluxo GetById em GamesController para Id {Id}", id);
        _logger.LogInformation("Recuperando jogo com Id {Id}", id);
        var game = await _gameService.GetByIdAsync(id);
        if (game is null)
        {
            _logger.LogWarning("Jogo não encontrado com Id {Id}", id);
            return NotFound();
        }
        _logger.LogTrace("Fluxo GetById finalizado em GamesController para Id {Id}", id);
        _logger.LogInformation("Jogo {GameName} recuperado com sucesso", game.Name);
        return Ok(game);
    }

    [HttpGet("/api/ListGamesByUserId/{userId}")]
    public async Task<ActionResult<IEnumerable<UserGameGameDto>>> GetbyUserId(string userId)
    {
        _logger.LogTrace("Iniciando fluxo GetbyUserId em GamesController para UserId {UserId}", userId);
        _logger.LogInformation("Recuperando jogos do usuário {UserId}", userId);
        var games = await _gameService.GetByUserIdAsync(userId);
        _logger.LogTrace("Fluxo GetbyUserId finalizado em GamesController para UserId {UserId}", userId);
        _logger.LogInformation("Usuário {UserId} possui {Count} jogos", userId, games.Count());
        return Ok(games);
    }

    [HttpGet("/api/TopGames")]
    public async Task<ActionResult<IEnumerable<TopGameDto>>> GetTopGames()
    {
        _logger.LogTrace("Iniciando fluxo GetTopGames em GamesController");
        var games = await _gameService.GetTopGamesAsync();
        _logger.LogTrace("Fluxo GetTopGames finalizado em GamesController com {Count} itens", games.Count());
        return Ok(games);
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        _logger.LogTrace("Iniciando fluxo Create em GamesController para jogo {GameName}", dto.Name);
        _logger.LogInformation("Criando novo jogo: {GameName}", dto.Name);
        var game = await _gameService.CreateAsync(dto);
        _logger.LogTrace("Fluxo Create finalizado em GamesController para jogo {GameName}", game.Name);
        _logger.LogInformation("Jogo criado com sucesso. Id: {GameId}", game.Id);
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateGameDto dto)
    {
        _logger.LogTrace("Iniciando fluxo Update em GamesController para Id {Id}", id);
        _logger.LogInformation("Atualizando jogo com Id {Id}: {GameName}", id, dto.Name);
        var updated = await _gameService.UpdateAsync(id, dto);
        if (!updated)
        {
            _logger.LogWarning("Jogo não encontrado para atualização com Id {Id}", id);
            return NotFound();
        }
        _logger.LogTrace("Fluxo Update finalizado em GamesController para Id {Id}", id);
        _logger.LogInformation("Jogo {Id} atualizado com sucesso", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        _logger.LogTrace("Iniciando fluxo Delete em GamesController para Id {Id}", id);
        _logger.LogInformation("Deletando jogo com Id {Id}", id);
        var deleted = await _gameService.DeleteAsync(id);
        if (!deleted)
        {
            _logger.LogWarning("Jogo não encontrado para deletar com Id {Id}", id);
            return NotFound();
        }
        _logger.LogTrace("Fluxo Delete finalizado em GamesController para Id {Id}", id);
        _logger.LogInformation("Jogo {Id} deletado com sucesso", id);
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<GameDto>>> Search([FromQuery] string q)
    {
        _logger.LogTrace("Iniciando fluxo Search em GamesController para query '{Query}'", q);
        _logger.LogInformation("Buscando jogos com query '{Query}'", q);
        var games = await _gameSearchService.SearchAsync(q);
        _logger.LogTrace("Fluxo Search finalizado em GamesController para query '{Query}'", q);
        return Ok(games);
    }

    [HttpPost("internal/reindex")]
    public async Task<IActionResult> ReindexAllGames()
    {
        _logger.LogTrace("Iniciando fluxo ReindexAllGames em GamesController");
        _logger.LogInformation("Disparando reindexação completa de jogos no Elasticsearch");

        var indexedCount = await _gameSearchService.ReindexAllAsync();

        _logger.LogInformation("Reindexação concluída com {Count} jogo(s) indexado(s)", indexedCount);
        _logger.LogTrace("Fluxo ReindexAllGames finalizado em GamesController");

        return Ok(new { indexed = indexedCount });
    }

    [HttpPost("{id}/reviews")]
    public async Task<ActionResult<GameReviewDto>> AddReview(string id, [FromBody] CreateGameReviewDto dto)
    {
        _logger.LogTrace("Iniciando fluxo AddReview em GamesController para GameId {GameId}", id);
        _logger.LogInformation("Adicionando avaliação para jogo {GameId} pelo usuário {UserId}", id, dto.UserId);

        try
        {
            var review = await _gameReviewService.AddReviewAsync(id, dto);

            _logger.LogTrace("Fluxo AddReview finalizado em GamesController para GameId {GameId}", id);
            return CreatedAtAction(nameof(GetReviewsByGameId), new { id }, review);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Nota inválida ao adicionar avaliação para GameId {GameId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/reviews")]
    public async Task<ActionResult<IEnumerable<GameReviewDto>>> GetReviewsByGameId(string id)
    {
        _logger.LogTrace("Iniciando fluxo GetReviewsByGameId em GamesController para GameId {GameId}", id);
        _logger.LogInformation("Listando avaliações para jogo {GameId}", id);

        var reviews = await _gameReviewService.GetReviewsByGameIdAsync(id);

        _logger.LogTrace("Fluxo GetReviewsByGameId finalizado em GamesController para GameId {GameId}", id);
        _logger.LogInformation("Foram retornadas {Count} avaliações para GameId {GameId}", reviews.Count(), id);
        return Ok(reviews);
    }
}
