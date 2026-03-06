using Catalog.Domain.Entity;
using Catalog.Domain.Interfaces;
using Catalog.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Infra.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Order order)
    {
        _logger.LogInformation("Adicionando novo pedido no banco de dados. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}", order.Id, order.UserId, order.GameId);
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pedido adicionado com sucesso. OrderId: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar pedido. OrderId: {OrderId}", order.Id);
            throw;
        }
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        _logger.LogDebug("Buscando pedido com Id {OrderId} no banco de dados", id);
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null)
                _logger.LogDebug("Pedido não encontrado com Id {OrderId}", id);
            else
                _logger.LogDebug("Pedido encontrado. OrderId: {OrderId}, Status: {Status}", id, order.Status);
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedido com Id {OrderId}", id);
            throw;
        }
    }

    public async Task UpdateAsync(Order order)
    {
        _logger.LogInformation("Atualizando pedido no banco de dados. OrderId: {OrderId}, Status: {Status}", order.Id, order.Status);
        try
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Pedido atualizado com sucesso. OrderId: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar pedido com Id {OrderId}", order.Id);
            throw;
        }
    }
}
