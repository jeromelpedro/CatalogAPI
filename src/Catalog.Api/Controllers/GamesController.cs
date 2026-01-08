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

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
    {
        var games = await _gameService.GetAllAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetById(string id)
    {
        var game = await _gameService.GetByIdAsync(id);
        return game is not null ? Ok(game) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> Create([FromBody] CreateGameDto dto)
    {
        var game = await _gameService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] CreateGameDto dto)
    {
        var updated = await _gameService.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _gameService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
