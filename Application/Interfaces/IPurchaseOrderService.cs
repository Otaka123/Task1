using Application.Common;
using Domain.DTOS.Filters;
using Domain.DTOS.Purchase;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrder?> GetByIdAsync(int id);
        Task<List<PurchaseOrder>> GetAllAsync();
        Task<PurchaseOrder> CreateAsync(PurchaseOrder order);
        Task<PurchaseOrder> UpdateAsync(PurchaseOrder order);
        Task<bool> DeleteAsync(int id);
        Task<PurchaseOrderDto?> GetByIdWithItemsAsync(int id);
        Task<ViewPurchaseOrderDto?> GetByIdWithItemssAsync(int id);
        Task<RequestResponse<PurchaseOrderDto>> UpdatePurchaseOrderAsync(UpdatePurchaseOrderDto updateDto);
        Task<List<PurchaseOrderItem>> GetItemsByOrderIdAsync(int orderId);
        Task<RequestResponse<PurchaseOrderDto>> CreatePurchaseOrderAsync(CreatePurchaseOrderDto createDto);
        
            Task<RequestResponse<PagedResultN<PurchaseOrderDto>>> GetAllPurchaseOrdersPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    int? supplierId = null,
    DateTime? fromDate = null,
    DateTime? toDate = null);
    }
}
