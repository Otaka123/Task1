using Domain.DTOS.Purchase;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface IPurchaseOrderRepository
    {
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<List<PurchaseOrder>> GetAllAsync();
        Task AddAsync(PurchaseOrder order);
        void Update(PurchaseOrder order);
        void Remove(PurchaseOrder order);
        Task<int> SaveAsync();
            Task<PurchaseOrder> CreateWithItemsAsync(PurchaseOrder order);
        Task<PurchaseOrder> GetByIdWithItemsAsync(int id);
        Task<PurchaseOrder> UpdateWithItemsAsync(PurchaseOrder order);
        Task<List<PurchaseOrderItem>> GetItemsByOrderIdAsync(int orderId);
        Task<int> GetPurchaseOrdersCountAsync(
            int? supplierId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<(List<PurchaseOrderDto> Data, int TotalCount)> GetAllPurchaseOrdersPagedWithCountAsync(
           int pageNumber = 1,
           int pageSize = 10,
           int? supplierId = null,
           DateTime? fromDate = null,
           DateTime? toDate = null);
        Task<PurchaseOrder> CreateWithItemsAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default);
        Task AddSignatureAsync(Signature signature);
        Task<Signature?> GetSignatureByPurchaseOrderIdAsync(int purchaseOrderId);
             Task<Signature?> GetSignatureByIdAsync(int signatureId);
        Task<bool> SignatureExistsAsync(int signatureId);
        Task<bool> IsPurchaseOrderSignedAsync(int purchaseOrderId);
        Task<PurchaseOrder?> GetByIdWithSignatureAsync(int id);

    }
}
