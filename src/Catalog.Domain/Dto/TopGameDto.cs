using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalog.Domain.Dto
{
    public class TopGameDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public int TotalPurchases { get; set; }
    }
}
