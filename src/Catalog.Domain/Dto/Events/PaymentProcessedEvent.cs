using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Dto.Events
{
    public enum PaymentStatus
    {
        Approved,
        Rejected
    }

    public class PaymentProcessedEvent
    {
        public string OrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
