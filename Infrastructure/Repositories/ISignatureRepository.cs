using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface  ISignatureRepository
    {
        Task<Signature?> GetByPurchaseOrderIdAsync(int purchaseOrderId);
        Task<Signature?> GetByIdAsync(int id);
        Task AddAsync(Signature signature);
        void Update(Signature signature);
        void Remove(Signature signature);
        Task<bool> SaveChangesAsync();
        Task<bool> ExistsAsync(int purchaseOrderId);
        Task<Signature?> GetByAdminIdAsync(string adminId);
    }
}
