using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Signature
    {
        public int Id { get; set; }
        public string SignaturePath { get; set; }
        public string AdminId { get; set; }

        // إضافة العلاقة مع PurchaseOrder
        public DateTime SignedDate { get; set; }           // تاريخ التوقيع
        public string? OTPCode { get; set; }               // كود OTP (اختياري)
        public DateTime? OTPExpiry { get; set; }           // تاريخ انتهاء OTP

        // Navigation Properties
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

        public Signature()
        {
            SignedDate = DateTime.UtcNow;
        }

        public bool IsOTPValid()
        {
            return OTPExpiry.HasValue && OTPExpiry.Value > DateTime.UtcNow;
        }
    }
}
