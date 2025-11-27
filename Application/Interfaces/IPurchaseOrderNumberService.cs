using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPurchaseOrderNumberService
    {
        Task<int> GetNextPurchaseOrderNumberAsync();
        Task<bool> IsPurchaseNumberExistsAsync(int purchaseNumber);
        Task<bool> IsPurchaseNumberExistsAsync(int purchaseNumber, int excludePurchaseOrderId);

    }
}
