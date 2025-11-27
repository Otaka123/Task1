using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class PurchaseOrder
    {
        public int Id { get; set; }                        // رقم أمر الشراء
        public DateTime PurchaseDate { get; set; }
        public int PurchaseNumber { get; set; }
        public bool HasVAT { get; set; } = false;

        // FK to Supplier
        public int SupplierId { get; set; }                // المفتاح الأجنبي للمورد
        public Supplier Supplier { get; set; } = null!;    // Navigation

        // العلاقة مع التوقيعات (واحد لواحد - اختيارية)
        public int? SignatureId { get; set; }

        // Navigation
        public Signature? Signature { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

        // خاصية محسوبة للتحقق من وجود توقيع
        public bool IsSigned => Signature != null;
        public string? GetDesignation()
        {
            return "مسؤول النظام";
        }
        // دالة مساعدة للحصول على بيانات الموقع
        //public string? GetAuthorizedByName()
        //{
        //    return Signature?.Admin?.FirstName + " " + Signature?.Admin?.LastName;
        //}
    }
}
