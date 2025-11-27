using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPurchaseOrderItemService
    {
        Task<PurchaseOrderItem?> GetByIdAsync(int id);
        Task<List<PurchaseOrderItem>> GetAllAsync();
        Task<List<PurchaseOrderItem>> GetByOrderIdAsync(int orderId);
        Task<PurchaseOrderItem> CreateAsync(PurchaseOrderItem item);
        Task<PurchaseOrderItem> UpdateAsync(PurchaseOrderItem item);
        Task<bool> DeleteAsync(int id);
    }
}
