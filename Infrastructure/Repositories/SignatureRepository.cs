using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SignatureRepository : ISignatureRepository
    {
        private readonly AppDbContext _context;

        public SignatureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Signature?> GetByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            return await _context.Signatures
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.PurchaseOrders.Any(p => p.Id == purchaseOrderId));
        }


        public async Task<Signature?> GetByIdAsync(int id)
        {
            return await _context.Signatures
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        public async Task AddAsync(Signature signature)
        {
            await _context.Signatures.AddAsync(signature);
        }

        public void Update(Signature signature)
        {
            _context.Signatures.Update(signature);
        }

        public void Remove(Signature signature)
        {
            _context.Signatures.Remove(signature);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
        // في AdminRepository
        public async Task<Signature?> GetByAdminIdAsync(string adminId)
        {
            return await _context.Signatures
                .FirstOrDefaultAsync(s => s.AdminId == adminId);
        }
        public async Task<bool> ExistsAsync(int purchaseOrderId)
        {
            return await _context.Signatures
                .AnyAsync(s => s.PurchaseOrders.Any(p => p.Id == purchaseOrderId));
        }

    }
}
