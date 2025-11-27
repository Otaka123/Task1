using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public interface ISupplierRepository
    {
        Task<Supplier> GetByIdAsync(int id);
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier> CreateAsync(Supplier supplier);
        Task<Supplier> UpdateAsync(Supplier supplier);
        Task<bool> DeleteAsync(int id);
    }
}
