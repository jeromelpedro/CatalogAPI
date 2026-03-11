using Catalog.Application.Interfaces;
using Catalog.Domain.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    // inicia o fluxo de compra
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        _logger.LogTrace("Iniciando fluxo Create em OrdersController para UserId {UserId} e GameId {GameId}", dto.UserId, dto.GameId);
        _logger.LogInformation("Iniciando criação de pedido para Usuário {UserId} - Jogo {GameId}", dto.UserId, dto.GameId);
        try
        {
            var order = await _orderService.CreateOrderAsync(dto);
            _logger.LogTrace("Fluxo Create em OrdersController finalizado para OrderId {OrderId}", order.Id);
            _logger.LogInformation("Pedido criado com sucesso. OrderId: {OrderId}, Status: {Status}", order.Id, order.Status);
            return Accepted(new
            {
                order.Id,
                order.UserId,
                order.GameId,
                order.Price,
                order.Status,
                order.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogTrace("Fluxo Create em OrdersController falhou para UserId {UserId}", dto.UserId);
            _logger.LogError(ex, "Erro ao criar pedido para Usuário {UserId}", dto.UserId);
            throw;
        }
    }
}
