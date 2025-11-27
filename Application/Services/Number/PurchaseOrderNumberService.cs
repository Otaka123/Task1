using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Number
{
    public class PurchaseOrderNumberService: IPurchaseOrderNumberService

    {
        //private readonly AppDbContext _context;
        //private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        //public PurchaseOrderNumberService(AppDbContext context)
        //{
        //    _context = context;
        //}

        //public async Task<int> GetNextPurchaseOrderNumberAsync()
        //{
        //    await _semaphore.WaitAsync();
        //    try
        //    {
        //        // البحث عن آخر رقم مستخدم
        //        var lastNumber = await _context.PurchaseOrders
        //            .MaxAsync(p => (int?)p.PurchaseNumber) ?? 0;

        //        return lastNumber + 1;
        //    }
        //    finally
        //    {
        //        _semaphore.Release();
        //    }
        //}
        private readonly AppDbContext _context;

        public PurchaseOrderNumberService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetNextPurchaseOrderNumberAsync()
        {
            // استخدام SQL مباشرة للحصول على الرقم التالي
            // ببساطة: ابحث عن آخر رقم وأضف 1
            var lastNumber = await _context.PurchaseOrders
                .AsNoTracking()
                .MaxAsync(p => (int?)p.PurchaseNumber) ?? 1000;

            return lastNumber + 1;
        }
        public async Task<bool> IsPurchaseNumberExistsAsync(int purchaseNumber)
        {
            return await _context.PurchaseOrders
                .AnyAsync(p => p.PurchaseNumber == purchaseNumber);
        }
        public async Task<bool> IsPurchaseNumberExistsAsync(int purchaseNumber, int excludePurchaseOrderId)
        {
            return await _context.PurchaseOrders
                .AnyAsync(p => p.PurchaseNumber == purchaseNumber && p.Id != excludePurchaseOrderId);
        }

        // دالة إضافية للحصول على الرقم التالي مع التحقق
        public async Task<int> GetNextValidPurchaseNumberAsync()
        {
            var nextNumber = await GetNextPurchaseOrderNumberAsync();

            // تحقق إضافي للتأكد
            while (await IsPurchaseNumberExistsAsync(nextNumber))
            {
                nextNumber++;
            }

            return nextNumber;
        }
    }
}
