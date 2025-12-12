using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Entity
{
    public class UserGame
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    }
}
