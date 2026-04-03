using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.Repositories;

public class ElasticsearchGameRepository : IGameSearchRepository
{
    private const string IndexName = "games";

    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchGameRepository> _logger;

    public ElasticsearchGameRepository(ElasticsearchClient client, ILogger<ElasticsearchGameRepository> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync(Game game)
    {
        _logger.LogTrace("Iniciando IndexAsync em ElasticsearchGameRepository para Id {Id}", game.Id);
        try
        {
            var document = new GameSearchDocument
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                Price = game.Price,
                PromotionalPrice = game.PromotionalPrice,
                Published = game.Published,
                Active = game.Active
            };

            var response = await _client.IndexAsync(document, idx => idx.Index(IndexName).Id(game.Id));

            if (!response.IsValidResponse)
                _logger.LogWarning("Falha ao indexar jogo Id {Id} no Elasticsearch: {Error}", game.Id, response.ElasticsearchServerError?.Error?.Reason);
            else
                _logger.LogInformation("Jogo Id {Id} indexado com sucesso no Elasticsearch", game.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao indexar jogo Id {Id} no Elasticsearch", game.Id);
            throw;
        }
    }

    public async Task<IEnumerable<GameDto>> SearchAsync(string query)
    {
        _logger.LogTrace("Iniciando SearchAsync em ElasticsearchGameRepository para query '{Query}'", query);
        try
        {
            var response = await _client.SearchAsync<GameSearchDocument>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Fields(new[] { "name", "genre" })
                        .Query(query)
                        .Fuzziness(new Fuzziness("AUTO"))
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Resposta inválida do Elasticsearch para query '{Query}': {Error}", query, response.ElasticsearchServerError?.Error?.Reason);
                return Enumerable.Empty<GameDto>();
            }

            var result = response.Hits.Select(h => new GameDto
            {
                Id = h.Source!.Id,
                Name = h.Source.Name,
                Genre = h.Source.Genre,
                Price = h.Source.Price,
                PromotionalPrice = h.Source.PromotionalPrice,
                Published = h.Source.Published,
                Active = h.Source.Active
            });

            _logger.LogInformation("Elasticsearch retornou {Count} resultado(s) para query '{Query}'", response.Hits.Count, query);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar jogos no Elasticsearch. Query: '{Query}'", query);
            throw;
        }
    }

    private sealed class GameSearchDocument
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PromotionalPrice { get; set; }
        public bool Published { get; set; }
        public bool Active { get; set; }
    }
}
