using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PurchaseOrderItem
    {
        public int Id { get; set; }

        public int Quantity { get; set; }
        [Precision(18, 2)]
        // الكمية
        public decimal Price { get; set; }    // وحدة القياس (مثلاً: كرتونة، كجم...)
        public string Description { get; set; } = null!; // البيان/الوصف

        // FK to PurchaseOrder
        public int PurchaseOrderId { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; } = null!;
    }

}
