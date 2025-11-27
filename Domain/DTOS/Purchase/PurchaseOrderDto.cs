using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int SupplierId { get; set; }
        public int PurchaseNumber { get; set; } // ← تأكد من وجوده
        public bool HasVAT { get; set; } // ← تأكد من وجوده

        public string SupplierName { get; set; } = null!;
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; } // ← إضافة
        public decimal TotalAmountWithVAT { get; set; } 
    }
}
