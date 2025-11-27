using System;
using System.Collections.Generic;
using System.Globalization;
using Humanizer;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    public class ViewPurchaseOrderDto
    {
        public int Id { get; set; }
        public string PurchaseOrderCode { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchaseOrderItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string TotalAmountInWords { get; set; }
        public bool HasVAT { get; set; }

        // الخصائص الاختيارية للتوقيع
        public string? AuthorizedBy { get; set; }
        public string? Designation { get; set; }
        public bool IsSigned { get; set; }
        public string? SignaturePath { get; set; }
        public DateTime? SignedDate { get; set; }
        public string ToWords()
        {
            decimal total = GrandTotal;
            long dirhams = (long)Math.Floor(total);
            int fils = (int)((total - dirhams) * 100);

            string dirhamsWords = Humanizer.NumberToWordsExtension.ToWords(dirhams, new CultureInfo("en"));

            if (fils > 0)
            {
                string filsWords = Humanizer.NumberToWordsExtension.ToWords(fils, new CultureInfo("en"));
                return string.Format("Amount: AED {0} Dirhams and {1} Fils", dirhamsWords, filsWords);
            }
            else
            {
                return string.Format("Amount: AED {0} Dirhams Only", dirhamsWords);
            }
        }
    }
}
