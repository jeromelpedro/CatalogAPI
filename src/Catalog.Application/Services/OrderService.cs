using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Catalog.Domain.Dto.Events;
using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Services;

public class OrderService(IOrderRepository _orderRepository, IGameRepository _gameRepository, 
    IUserGameRepository _userGameRepository, IServiceBus _serviceBus, IConfiguration _configuration , ILogger<OrderService> _logger) : IOrderService
{    
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        _logger.LogTrace("Iniciando CreateOrderAsync para UserId {UserId}, GameId {GameId}", dto.UserId, dto.GameId);
        _logger.LogInformation("Iniciando criação de pedido. Usuário: {UserId}, Jogo: {GameId}", dto.UserId, dto.GameId);
        try
        {
			string OrderPlacedQueueName = _configuration["ServiceBus:QueueNameOrderPlaced"];

			var game = await _gameRepository.GetByIdAsync(dto.GameId);
            if (game is null || !game.Active || !game.Published)
            {
                _logger.LogWarning("Jogo inválido para compra. GameId: {GameId}, Ativo: {Active}, Publicado: {Published}", dto.GameId, game?.Active, game?.Published);
                throw new InvalidOperationException("Jogo inválido para compra.");
            }

            var price = game.PromotionalPrice > 0 ? game.PromotionalPrice : game.Price;
            _logger.LogTrace("Preço definido para pedido. Price: {Price}, PromotionalPrice: {PromotionalPrice}, BasePrice: {BasePrice}", price, game.PromotionalPrice, game.Price);

            var order = new Order
            {
                UserId = dto.UserId,
                GameId = dto.GameId,
                Price = price,
                Status = OrderStatus.Pending
            };

            await _orderRepository.AddAsync(order);
            _logger.LogTrace("Pedido persistido em CreateOrderAsync com OrderId {OrderId}", order.Id);
            _logger.LogInformation("Pedido criado com sucesso. OrderId: {OrderId}, Preço: {Price}", order.Id, price);

            var evt = new OrderPlacedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                GameId = order.GameId,
                EmailUser = dto.EmailUser,
                Price = order.Price,
                CreatedAt = order.CreatedAt
            };

            _logger.LogInformation("Publicando OrderPlacedEvent na fila {QueueName} para OrderId {OrderId}", OrderPlacedQueueName, order.Id);
            await _serviceBus.PublishAsync(OrderPlacedQueueName, evt);
            _logger.LogTrace("OrderPlacedEvent publicado para OrderId {OrderId}", order.Id);

            _logger.LogTrace("Finalizando CreateOrderAsync para OrderId {OrderId}", order.Id);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido para usuário {UserId}", dto.UserId);
            throw;
        }
    }

    // chamado pelo consumidor de PaymentProcessedEvent quando Approved
    public async Task AddGameToUserLibraryAsync(PaymentProcessedEvent evt)
    {
        _logger.LogTrace("Iniciando AddGameToUserLibraryAsync para OrderId {OrderId}", evt.OrderId);
        _logger.LogInformation("Processando addição de jogo à biblioteca. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}", evt.OrderId, evt.UserId, evt.GameId);
        try
        {
            if (evt.Status != PaymentStatus.Approved)
            {
                _logger.LogWarning("Pagamento não aprovado para OrderId {OrderId}. Status: {Status}", evt.OrderId, evt.Status);
                return;
            }

            var exists = await _userGameRepository.ExistsAsync(evt.UserId, evt.GameId);
            if (exists)
            {
                _logger.LogInformation("Jogo já está na biblioteca do usuário {UserId}", evt.UserId);
                return;
            }

            var userGame = new UserGame
            {
                UserId = evt.UserId,
                GameId = evt.GameId
            };
            await _userGameRepository.AddAsync(userGame);
            _logger.LogTrace("Registro de UserGame criado para UserId {UserId} e GameId {GameId}", evt.UserId, evt.GameId);
            _logger.LogInformation("Jogo {GameId} adicionado à biblioteca do usuário {UserId}", evt.GameId, evt.UserId);

            var order = await _orderRepository.GetByIdAsync(evt.OrderId);
            if (order is not null)
            {
                order.Status = OrderStatus.Approved;
                await _orderRepository.UpdateAsync(order);
                _logger.LogInformation("Status do pedido {OrderId} atualizado para Approved", evt.OrderId);
            }

            _logger.LogTrace("Finalizando AddGameToUserLibraryAsync para OrderId {OrderId}", evt.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar jogo à biblioteca. OrderId: {OrderId}, UserId: {UserId}", evt.OrderId, evt.UserId);
            throw;
        }
    }
}
