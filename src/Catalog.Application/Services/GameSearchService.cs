using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class GameSearchService : IGameSearchService
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameSearchRepository _searchRepository;
    private readonly ILogger<GameSearchService> _logger;

    public GameSearchService(IGameRepository gameRepository, IGameSearchRepository searchRepository, ILogger<GameSearchService> logger)
    {
        _gameRepository = gameRepository;
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<GameDto>> SearchAsync(string query)
    {
        _logger.LogTrace("Iniciando SearchAsync em GameSearchService para query '{Query}'", query);
        _logger.LogInformation("Buscando jogos no Elasticsearch com query '{Query}'", query);
        try
        {
            var result = await _searchRepository.SearchAsync(query);
            _logger.LogInformation("Busca finalizada. Query: '{Query}'", query);
            _logger.LogTrace("Finalizando SearchAsync em GameSearchService");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogos no Elasticsearch. Query: '{Query}'", query);
            throw;
        }
    }

    public async Task<int> ReindexAllAsync()
    {
        _logger.LogTrace("Iniciando ReindexAllAsync em GameSearchService");
        _logger.LogInformation("Iniciando reindexação completa da base de jogos");

        try
        {
            var games = await _gameRepository.GetAllAsync();
            var indexedCount = 0;

            foreach (var game in games)
            {
                try
                {
                    await _searchRepository.IndexAsync(game);
                    indexedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Falha ao reindexar jogo Id {Id}; seguindo com próximos itens", game.Id);
                }
            }

            _logger.LogInformation("Reindexação finalizada. Jogos lidos: {Total}; Jogos indexados: {Indexed}", games.Count, indexedCount);
            _logger.LogTrace("Finalizando ReindexAllAsync em GameSearchService");

            return indexedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante a reindexação completa da base de jogos");
            throw;
        }
    }
}
