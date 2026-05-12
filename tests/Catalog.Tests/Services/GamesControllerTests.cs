using Catalog.Api.Controllers;
using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Catalog.Tests.Controllers
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _gameServiceMock;
        private readonly Mock<IGameSearchService> _gameSearchServiceMock;
        private readonly Mock<IGameReviewService> _gameReviewServiceMock;
        private readonly Mock<ILogger<GamesController>> _loggerMock;
        private readonly GamesController _controller;

        public GamesControllerTests()
        {
            _gameServiceMock = new Mock<IGameService>();
            _gameSearchServiceMock = new Mock<IGameSearchService>();
            _gameReviewServiceMock = new Mock<IGameReviewService>();
            _loggerMock = new Mock<ILogger<GamesController>>();

            _controller = new GamesController(
                _gameServiceMock.Object,
                _gameSearchServiceMock.Object,
                _gameReviewServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAll_DeveRetornarOk_ComListaDeJogos()
        {
            var games = new List<GameDto>
            {
                new()
                {
                    Id = "1",
                    Name = "FIFA",
                    Genre = "Esporte",
                    Price = 100
                },
                new()
                {
                    Id = "2",
                    Name = "COD",
                    Genre = "FPS",
                    Price = 200
                }
            };

            _gameServiceMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(games);

            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGames = Assert.IsAssignableFrom<IEnumerable<GameDto>>(okResult.Value);

            Assert.Equal(2, returnedGames.Count());

            _gameServiceMock.Verify(
                x => x.GetAllAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task GetById_DeveRetornarOk_QuandoJogoExiste()
        {
            var game = new GameDto
            {
                Id = "1",
                Name = "FIFA",
                Genre = "Esporte",
                Price = 100
            };

            _gameServiceMock
                .Setup(x => x.GetByIdAsync("1"))
                .ReturnsAsync(game);

            var result = await _controller.GetById("1");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGame = Assert.IsType<GameDto>(okResult.Value);

            Assert.Equal(game.Id, returnedGame.Id);
            Assert.Equal(game.Name, returnedGame.Name);

            _gameServiceMock.Verify(
                x => x.GetByIdAsync("1"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetById_DeveRetornarNotFound_QuandoJogoNaoExiste()
        {
            _gameServiceMock
                .Setup(x => x.GetByIdAsync("999"))
                .ReturnsAsync((GameDto?)null);

            var result = await _controller.GetById("999");

            Assert.IsType<NotFoundResult>(result.Result);

            _gameServiceMock.Verify(
                x => x.GetByIdAsync("999"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetByUserId_DeveRetornarJogosDoUsuario()
        {
            var games = new List<UserGameGameDto>
            {
                new()
                {
                    Id = "1",
                    Name = "FIFA",
                    Genre = "Esporte"
                }
            };

            _gameServiceMock
                .Setup(x => x.GetByUserIdAsync("user-1"))
                .ReturnsAsync(games);

            var result = await _controller.GetbyUserId("user-1");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGames =
                Assert.IsAssignableFrom<IEnumerable<UserGameGameDto>>(okResult.Value);

            Assert.Single(returnedGames);

            _gameServiceMock.Verify(
                x => x.GetByUserIdAsync("user-1"),
                Times.Once
            );
        }

        [Fact]
        public async Task GetTopGames_DeveRetornarLista()
        {
            var games = new List<TopGameDto>
            {
                new()
                {
                    Id = "1",
                    Name = "FIFA",
                    Genre = "Esporte",
                    TotalPurchases = 50
                }
            };

            _gameServiceMock
                .Setup(x => x.GetTopGamesAsync())
                .ReturnsAsync(games);

            var result = await _controller.GetTopGames();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGames =
                Assert.IsAssignableFrom<IEnumerable<TopGameDto>>(okResult.Value);

            Assert.Single(returnedGames);

            _gameServiceMock.Verify(
                x => x.GetTopGamesAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task Create_DeveRetornarCreatedAtAction()
        {
            var dto = new CreateGameDto
            {
                Name = "FIFA",
                Genre = "Esporte",
                Price = 100
            };

            var createdGame = new GameDto
            {
                Id = "1",
                Name = dto.Name,
                Genre = dto.Genre,
                Price = dto.Price
            };

            _gameServiceMock
                .Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(createdGame);

            var result = await _controller.Create(dto);
            var createdResult =
                Assert.IsType<CreatedAtActionResult>(result.Result);

            var game =
                Assert.IsType<GameDto>(createdResult.Value);

            Assert.Equal(createdGame.Id, game.Id);

            _gameServiceMock.Verify(
                x => x.CreateAsync(dto),
                Times.Once
            );
        }

        [Fact]
        public async Task Update_DeveRetornarNoContent_QuandoAtualizado()
        {
            var dto = new CreateGameDto
            {
                Name = "Novo FIFA"
            };

            _gameServiceMock
                .Setup(x => x.UpdateAsync("1", dto))
                .ReturnsAsync(true);

            var result = await _controller.Update("1", dto);

            Assert.IsType<NoContentResult>(result);

            _gameServiceMock.Verify(
                x => x.UpdateAsync("1", dto),
                Times.Once
            );
        }

        [Fact]
        public async Task Update_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            var dto = new CreateGameDto
            {
                Name = "Novo FIFA"
            };

            _gameServiceMock
                .Setup(x => x.UpdateAsync("999", dto))
                .ReturnsAsync(false);

            var result = await _controller.Update("999", dto);

            Assert.IsType<NotFoundResult>(result);

            _gameServiceMock.Verify(
                x => x.UpdateAsync("999", dto),
                Times.Once
            );
        }

        [Fact]
        public async Task Delete_DeveRetornarNoContent_QuandoExcluido()
        {
            _gameServiceMock
                .Setup(x => x.DeleteAsync("1"))
                .ReturnsAsync(true);

            var result = await _controller.Delete("1");

            Assert.IsType<NoContentResult>(result);

            _gameServiceMock.Verify(
                x => x.DeleteAsync("1"),
                Times.Once
            );
        }

        [Fact]
        public async Task Delete_DeveRetornarNotFound_QuandoNaoEncontrado()
        {
            _gameServiceMock
                .Setup(x => x.DeleteAsync("999"))
                .ReturnsAsync(false);

            var result = await _controller.Delete("999");

            Assert.IsType<NotFoundResult>(result);

            _gameServiceMock.Verify(
                x => x.DeleteAsync("999"),
                Times.Once
            );
        }

        [Fact]
        public async Task Search_DeveRetornarJogos()
        {
            var games = new List<GameDto>
            {
                new()
                {
                    Id = "1",
                    Name = "FIFA",
                    Genre = "Esporte",
                    Price = 100
                }
            };

            _gameSearchServiceMock
                .Setup(x => x.SearchAsync("FIFA"))
                .ReturnsAsync(games);

            var result = await _controller.Search("FIFA");
            var okResult =
                Assert.IsType<OkObjectResult>(result.Result);

            var returnedGames =
                Assert.IsAssignableFrom<IEnumerable<GameDto>>(
                    okResult.Value);

            Assert.Single(returnedGames);

            _gameSearchServiceMock.Verify(
                x => x.SearchAsync("FIFA"),
                Times.Once
            );
        }

        [Fact]
        public async Task ReindexAllGames_DeveRetornarQuantidadeIndexada()
        {
            _gameSearchServiceMock
                .Setup(x => x.ReindexAllAsync())
                .ReturnsAsync(10);

            var result = await _controller.ReindexAllGames();
            var okResult =
                Assert.IsType<OkObjectResult>(result);

            Assert.NotNull(okResult.Value);

            _gameSearchServiceMock.Verify(
                x => x.ReindexAllAsync(),
                Times.Once
            );
        }

        [Fact]
        public async Task AddReview_DeveRetornarCreatedAtAction()
        {
            var dto = new CreateGameReviewDto
            {
                UserId = "user-1",
                Rating = 5,
                Comment = "Excelente jogo"
            };

            var review = new GameReviewDto
            {
                Id = "review-1",
                GameId = "game-1",
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _gameReviewServiceMock
                .Setup(x => x.AddReviewAsync("game-1", dto))
                .ReturnsAsync(review);

            var result =
                await _controller.AddReview("game-1", dto);

            var createdResult =
                Assert.IsType<CreatedAtActionResult>(
                    result.Result);

            var returnedReview =
                Assert.IsType<GameReviewDto>(
                    createdResult.Value);

            Assert.Equal(
                review.Id,
                returnedReview.Id);

            _gameReviewServiceMock.Verify(
                x => x.AddReviewAsync("game-1", dto),
                Times.Once
            );
        }

        [Fact]
        public async Task AddReview_DeveRetornarBadRequest_QuandoNotaInvalida()
        {
            var dto = new CreateGameReviewDto
            {
                UserId = "user-1",
                Rating = 10
            };

            _gameReviewServiceMock
                .Setup(x => x.AddReviewAsync("game-1", dto))
                .ThrowsAsync(
                    new ArgumentOutOfRangeException(
                        "Rating",
                        "Nota inválida"));

            var result =
                await _controller.AddReview("game-1", dto);

            var badRequest =
                Assert.IsType<BadRequestObjectResult>(
                    result.Result);

            Assert.NotNull(badRequest.Value);

            _gameReviewServiceMock.Verify(
                x => x.AddReviewAsync("game-1", dto),
                Times.Once
            );
        }

        [Fact]
        public async Task GetReviewsByGameId_DeveRetornarLista()
        {
            var reviews = new List<GameReviewDto>
            {
                new()
                {
                    Id = "review-1",
                    GameId = "game-1",
                    UserId = "user-1",
                    Rating = 5,
                    Comment = "Excelente"
                }
            };

            _gameReviewServiceMock
                .Setup(x =>
                    x.GetReviewsByGameIdAsync("game-1"))
                .ReturnsAsync(reviews);

            var result =
                await _controller
                    .GetReviewsByGameId("game-1");

            var okResult =
                Assert.IsType<OkObjectResult>(
                    result.Result);

            var returnedReviews =
                Assert.IsAssignableFrom<
                    IEnumerable<GameReviewDto>>(
                    okResult.Value);

            Assert.Single(returnedReviews);

            _gameReviewServiceMock.Verify(
                x => x.GetReviewsByGameIdAsync("game-1"),
                Times.Once
            );
        }
    }
}