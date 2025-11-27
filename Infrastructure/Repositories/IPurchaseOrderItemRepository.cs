using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IPurchaseOrderItemRepository
    {
        Task<PurchaseOrderItem?> GetByIdAsync(int id);
        Task<List<PurchaseOrderItem>> GetAllAsync();
        Task<List<PurchaseOrderItem>> GetByOrderIdAsync(int orderId);
        Task AddAsync(PurchaseOrderItem item);
        void Update(PurchaseOrderItem item);
        void Remove(PurchaseOrderItem item);
        Task<int> SaveAsync();
    }
}
