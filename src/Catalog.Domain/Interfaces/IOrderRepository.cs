using Catalog.Domain.Entity;

namespace Catalog.Domain.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(string id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
    }
}
