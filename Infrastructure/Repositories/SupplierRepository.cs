using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly AppDbContext _context;

        public SupplierRepository(AppDbContext context) { 
            _context = context;
        }

        public async Task<Supplier> GetByIdAsync(int id)
        {
            var entity = await _context.Set<Supplier>().FirstOrDefaultAsync(S=>S.Id == id);
            if (entity == null)
                throw new KeyNotFoundException("Supplier not found");
            return entity;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _context.Set<Supplier>().OrderBy(S=>S.Id).ToListAsync();
        }

        public async Task<Supplier> CreateAsync(Supplier supplier)
        {
            await _context.Set<Supplier>().AddAsync(supplier);
            await _context.SaveChangesAsync();
            return supplier;
        }

        public async Task<Supplier> UpdateAsync(Supplier supplier)
        {
            var existing = await _context.Set<Supplier>().FirstOrDefaultAsync(S => S.Id == supplier.Id);
            if (existing == null) throw new KeyNotFoundException("Supplier not found");

            existing.Name = supplier.Name;
            _context.Set<Supplier>().Update(existing);
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.Set<Supplier>().FirstOrDefaultAsync(S=>S.Id==id);
            if (existing == null) return false;

            _context.Set<Supplier>().Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
