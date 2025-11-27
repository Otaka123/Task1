using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Purchase
{
    public class CreatePurchaseOrderItemDto
    {
        public int Id { get; set; }
        //public int PurchaseOrderId {  get; set; }
        [Required(ErrorMessage = "هذا الحقل مطلوب !")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "هذا الحقل مطلوب !")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب !")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]

        public decimal Price { get; set; }
        public decimal Total => this.Price * this.Quantity;
    }
}
