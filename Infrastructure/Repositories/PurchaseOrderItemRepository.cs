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
    public class PurchaseOrderItemRepository : IPurchaseOrderItemRepository
    {
        private readonly AppDbContext _context;

        public PurchaseOrderItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrderItem?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrderItems
                                 .Include(i => i.PurchaseOrder)
                                 .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<PurchaseOrderItem>> GetAllAsync()
        {
            return await _context.PurchaseOrderItems
                                 .Include(i => i.PurchaseOrder)
                                 .ToListAsync();
        }

        public async Task<List<PurchaseOrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _context.PurchaseOrderItems
                                 .Where(i => i.PurchaseOrderId == orderId)
                                 .ToListAsync();
        }

        public async Task AddAsync(PurchaseOrderItem item)
        {
            await _context.PurchaseOrderItems.AddAsync(item);
        }

        public void Update(PurchaseOrderItem item)
        {
            _context.PurchaseOrderItems.Update(item);
        }

        public void Remove(PurchaseOrderItem item)
        {
            _context.PurchaseOrderItems.Remove(item);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
