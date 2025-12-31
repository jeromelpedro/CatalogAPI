using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Dto.Events;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IUserGameRepository _userGameRepository;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    //private readonly ILogger<OrderService> _logger;

    private const string OrderPlacedQueueName = "OrderPlacedEvent";

    public OrderService(
        IOrderRepository orderRepository,
        IGameRepository gameRepository,
        IUserGameRepository userGameRepository,
        IRabbitMqPublisher rabbitMqPublisher)
        //ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _gameRepository = gameRepository;
        _userGameRepository = userGameRepository;
        _rabbitMqPublisher = rabbitMqPublisher;
        //_logger = logger;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        var game = await _gameRepository.GetByIdAsync(dto.GameId);
        if (game is null || !game.Active || !game.Published)
            throw new InvalidOperationException("Jogo inválido para compra.");

        var price = game.PromotionalPrice > 0 ? game.PromotionalPrice : game.Price;

        var order = new Order
        {
            UserId = dto.UserId,
            GameId = dto.GameId,
            Price = price,
            Status = OrderStatus.Pending
        };

        await _orderRepository.AddAsync(order);

        var evt = new OrderPlacedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            GameId = order.GameId,
            Price = order.Price,
            CreatedAt = order.CreatedAt
        };

        //_logger.LogInformation("Publicando OrderPlacedEvent para OrderId {OrderId}", order.Id);
        await _rabbitMqPublisher.PublishAsync(evt, OrderPlacedQueueName);

        return order;
    }

    // chamado pelo consumidor de PaymentProcessedEvent quando Approved
    public async Task AddGameToUserLibraryAsync(PaymentProcessedEvent evt)
    {
        if (evt.Status != PaymentStatus.Approved)
        {
            //_logger.LogWarning("Pagamento não aprovado para OrderId {OrderId}", evt.OrderId);
            return;
        }

        var exists = await _userGameRepository.ExistsAsync(evt.UserId, evt.GameId);
        if (exists)
        {
            //_logger.LogInformation("Jogo já está na biblioteca do usuário {UserId}", evt.UserId);
            return;
        }

        var userGame = new UserGame
        {
            UserId = evt.UserId,
            GameId = evt.GameId
        };
        await _userGameRepository.AddAsync(userGame);

        var order = await _orderRepository.GetByIdAsync(evt.OrderId);
        if (order is not null)
        {
            order.Status = OrderStatus.Approved;
            await _orderRepository.UpdateAsync(order);
        }

        //_logger.LogInformation("Jogo {GameId} adicionado à biblioteca do usuário {UserId}", evt.GameId, evt.UserId);
    }
}
