using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    //public class CreatePurchaseOrderDto
    //{
    //    [Required(ErrorMessage = "Purchase date is required")]
    //    public DateTime PurchaseDate { get; set; } = DateTime.Today;

    //    [Required(ErrorMessage = "Supplier is required")]
    //    [Range(1, int.MaxValue, ErrorMessage = "Please select a supplier")]
    //    public int SupplierId { get; set; }
    //    [Range(1, int.MaxValue, ErrorMessage = "Please select a supplier")]
    //    public int PurchaseNumber { get; set; } // ← تأكد من وجوده
    //    public bool HasVAT {  get; set; }=false;

    //    [Required(ErrorMessage = "At least one item is required")]
    //    [MinLength(1, ErrorMessage = "At least one item is required")]
    //    public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    //}
    public class CreatePurchaseOrderDto
    {
        [Required(ErrorMessage = "هذا الحقل مطلوب !")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "هذا الحقل مطلوب !")]
        [Range(1, int.MaxValue, ErrorMessage = "برجاء اختيار مورد")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "رقم أمر الشراء مطلوب")] // ← مطلوب
        [Range(1, int.MaxValue, ErrorMessage = "رقم أمر الشراء يجب ان يكون موجبأ")]
        public int PurchaseNumber { get; set; }

        public bool HasVAT { get; set; } = false;

        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    }
}
