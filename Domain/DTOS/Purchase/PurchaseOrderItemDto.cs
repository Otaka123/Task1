using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    public class PurchaseOrderItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal total => Price * Quantity;
        public string Description { get; set; } = null!;
        public int PurchaseOrderId { get; set; }
    }
}
