using Catalog.Domain.Dto;
using Catalog.Domain.Dto.Events;
using Catalog.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto dto);
        Task AddGameToUserLibraryAsync(PaymentProcessedEvent evt);
    }
}
