using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    public class UpdatePurchaseOrderDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "التاريخ مطلوب")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "المورد مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "برجاء اختيار مورد")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "رقم أمر الشراء مطلوب")] // ← مطلوب
        [Range(1, int.MaxValue, ErrorMessage = "رقم أمر الشراء يجب ان يكون موجبأ")]
        public int PurchaseNumber { get; set; }

        public bool HasVAT { get; set; } = false;

   
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    }
}

