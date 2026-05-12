using Catalog.Api.Controllers;
using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Catalog.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _orderServiceMock;
        private readonly Mock<ILogger<OrdersController>> _loggerMock;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _orderServiceMock = new Mock<IOrderService>();
            _loggerMock = new Mock<ILogger<OrdersController>>();

            _controller = new OrdersController(
                _orderServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Create_DeveRetornarAccepted_QuandoPedidoCriado()
        {
            var dto = new CreateOrderDto
            {
                UserId = "user-1",
                EmailUser = "teste@teste.com",
                GameId = "game-1"
            };

            var order = new Order
            {
                Id = "order-1",
                UserId = dto.UserId,
                GameId = dto.GameId,
                Price = 99.90m,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(dto))
                .ReturnsAsync(order);

            var result = await _controller.Create(dto);
            var acceptedResult = Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(acceptedResult.Value);

            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(dto),
                Times.Once
            );
        }

        [Fact]
        public async Task Create_DeveLancarExcecao_QuandoServiceFalhar()
        {
            var dto = new CreateOrderDto
            {
                UserId = "user-1",
                EmailUser = "teste@teste.com",
                GameId = "game-1"
            };

            _orderServiceMock
                .Setup(x => x.CreateOrderAsync(dto))
                .ThrowsAsync(new Exception("Erro interno"));

            await Assert.ThrowsAsync<Exception>(
                () => _controller.Create(dto)
            );

            _orderServiceMock.Verify(
                x => x.CreateOrderAsync(dto),
                Times.Once
            );
        }
    }
}