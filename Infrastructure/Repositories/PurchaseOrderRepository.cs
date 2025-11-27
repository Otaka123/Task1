using Domain.DTOS.Filters;
using Domain.DTOS.Purchase;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
    {
        private readonly AppDbContext _context;

        public PurchaseOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _context.PurchaseOrders
                                 .Include(o => o.Supplier)
                                 .Include(o => o.Items)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }
        public async Task<Signature?> GetSignatureByPurchaseOrderIdAsync(int purchaseOrderId)
        {
            return await _context.Signatures
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.PurchaseOrders.Any(p => p.Id == purchaseOrderId));
        }

        public async Task<Signature?> GetSignatureByIdAsync(int signatureId)
        {
            return await _context.Signatures
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.Id == signatureId);
        }

        public async Task<bool> IsPurchaseOrderSignedAsync(int purchaseOrderId)
        {
            return await _context.PurchaseOrders
                .AnyAsync(p => p.Id == purchaseOrderId && p.SignatureId != null);
        }


        public async Task<bool> SignatureExistsAsync(int signatureId)
        {
            return await _context.Signatures
                .AnyAsync(s => s.Id == signatureId);
        }
        //public async Task<List<PurchaseOrderDto>> GetAllPurchaseOrdersPagedAsync(
        //int pageNumber = 1,
        //int pageSize = 10,
        //int? supplierId = null,
        //DateTime? fromDate = null,
        //DateTime? toDate = null)
        //    {
        //        try
        //        {
        //            var query = _context.PurchaseOrders.AsNoTracking()
        //                .Include(po => po.Supplier)
        //                .Include(po => po.Items)
        //                .AsQueryable();

        //            // ✅ تطبيق الفلترة حسب المورد
        //            if (supplierId.HasValue)
        //            {
        //                query = query.Where(po => po.SupplierId == supplierId.Value);
        //            }

        //            // ✅ تطبيق الفلترة حسب الفترة الزمنية
        //            if (fromDate.HasValue)
        //            {
        //                query = query.Where(po => po.PurchaseDate >= fromDate.Value);
        //            }

        //            if (toDate.HasValue)
        //            {
        //                // نضيف يوم واحد ليشمل اليوم المحدد في toDate
        //                query = query.Where(po => po.PurchaseDate < toDate.Value.AddDays(1));
        //            }

        //            var totalCount = await query.CountAsync();

        //            var data = await query
        //                .OrderByDescending(po => po.PurchaseDate)
        //                .Skip((pageNumber - 1) * pageSize)
        //                .Take(pageSize)
        //                .Select(po => new PurchaseOrderDto
        //                {
        //                    Id = po.Id,
        //                    PurchaseDate = po.PurchaseDate,
        //                    SupplierId = po.SupplierId,
        //                    SupplierName = po.Supplier.Name, // افترض أن Supplier له خاصية Name
        //                    Items = po.Items.Select(item => new PurchaseOrderItemDto
        //                    {
        //                        Id = item.Id,
        //                        Quantity = item.Quantity,
        //                        Price = item.Price,
        //                        Description = item.Description,
        //                        PurchaseOrderId = item.PurchaseOrderId
        //                    }).ToList(),
        //                    TotalAmount = po.Items.Sum(item => item.Quantity * item.Price)
        //                })
        //                .ToListAsync();

        //            var pagedResult = new List<PurchaseOrderDto>(data);


        //            return pagedResult;
        //        }
        //        catch (Exception ex)
        //        {
        //            return new List<PurchaseOrderDto>();
        //        }
        //    }
        // في الـ Repository
        //public async Task<PurchaseOrder> CreateWithItemsAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        //{
        //    // إضافة أمر الشراء بكل بنوده
        //    await _context.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
        //    await _context.SaveChangesAsync(cancellationToken);

        //    return purchaseOrder;
        //}
        public async Task<PurchaseOrder> CreateWithItemsAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        {
            if (purchaseOrder == null)
                throw new ArgumentNullException(nameof(purchaseOrder));

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // إضافة أمر الشراء
                await _context.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // العناصر سيتم حفظها تلقائياً بسبب العلاقة
                await transaction.CommitAsync(cancellationToken);

                // إعادة تحميل الكائن مع البيانات المتعلقة به
                return await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id, cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<PurchaseOrder> UpdateWithItemsAsync(PurchaseOrder order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // التحقق من وجود أمر الشراء
                var existingOrder = await _context.PurchaseOrders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);

                if (existingOrder == null)
                    throw new ArgumentException("Purchase order not found");

                // تحديث البيانات الأساسية
                existingOrder.PurchaseDate = order.PurchaseDate;
                existingOrder.SupplierId = order.SupplierId;
                existingOrder.HasVAT = order.HasVAT;

                // مسح البنود القديمة
                _context.PurchaseOrderItems.RemoveRange(existingOrder.Items);

                // إضافة البنود الجديدة
                foreach (var item in order.Items)
                {
                    var newItem = new PurchaseOrderItem
                    {
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description,
                        PurchaseOrderId = existingOrder.Id
                    };
                    _context.PurchaseOrderItems.Add(newItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdWithItemsAsync(order.Id);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task UpdateOrderItems(PurchaseOrder existingOrder, ICollection<PurchaseOrderItem> newItems)
        {
            // الحصول على البنود الحالية
            var existingItems = existingOrder.Items.ToList();

            // تحديد البنود للحذف (البنود الموجودة في القائمة الحالية وغير موجودة في الجديدة)
            var itemsToDelete = existingItems
                .Where(existing => !newItems.Any(newItem => newItem.Id == existing.Id && newItem.Id != 0))
                .ToList();

            // تحديد البنود للإضافة (البنود الجديدة التي ليس لها Id)
            var itemsToAdd = newItems
                .Where(newItem => newItem.Id == 0)
                .ToList();

            // تحديد البنود للتحديث (البنود الموجودة في كلا القائمتين)
            var itemsToUpdate = newItems
                .Where(newItem => newItem.Id != 0 && existingItems.Any(existing => existing.Id == newItem.Id))
                .ToList();

            // تنفيذ العمليات
            await DeleteItemsAsync(itemsToDelete);
            await AddItemsAsync(existingOrder.Id, itemsToAdd);
            await UpdateItemsAsync(itemsToUpdate);
        }
        private async Task AddItemsAsync(int purchaseOrderId, List<PurchaseOrderItem> itemsToAdd)
        {
            foreach (var item in itemsToAdd)
            {
                item.PurchaseOrderId = purchaseOrderId;
                item.Id = 0; // التأكد من أن الـ Id صفر للإضافة الجديدة
                _context.PurchaseOrderItems.Add(item);
            }

            if (itemsToAdd.Any())
                await _context.SaveChangesAsync();
        }

        private async Task UpdateItemsAsync(List<PurchaseOrderItem> itemsToUpdate)
        {
            foreach (var updatedItem in itemsToUpdate)
            {
                var existingItem = await _context.PurchaseOrderItems
                    .FirstOrDefaultAsync(i => i.Id == updatedItem.Id);

                if (existingItem != null)
                {
                    _context.Entry(existingItem).CurrentValues.SetValues(new
                    {
                        updatedItem.Quantity,
                        updatedItem.Price,
                        updatedItem.Description
                    });
                }
            }

            if (itemsToUpdate.Any())
                await _context.SaveChangesAsync();
        }

        public async Task<PurchaseOrder> GetByIdWithItemsAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(o => o.Items)
                .Include(o => o.Supplier)
                .Include(o => o.Signature)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // في PurchaseOrderRepository
        public async Task<PurchaseOrder?> GetByIdWithSignatureAsync(int id)
        {
            return await _context.PurchaseOrders
                .Include(po => po.Signature)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task AddSignatureAsync(Signature signature)
        {
            await _context.Signatures.AddAsync(signature);
        }

        public async Task<PurchaseOrder> CreateWithItemsAsync(PurchaseOrder order)
        {
            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var item = await _context.PurchaseOrderItems.FindAsync(itemId);
            if (item == null) return false;

            _context.PurchaseOrderItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PurchaseOrderItem> AddItemAsync(PurchaseOrderItem item)
        {
            _context.PurchaseOrderItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }
        private async Task DeleteItemsAsync(List<PurchaseOrderItem> itemsToDelete)
        {
            if (itemsToDelete.Any())
            {
                _context.PurchaseOrderItems.RemoveRange(itemsToDelete);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<PurchaseOrderDto> Data, int TotalCount)> GetAllPurchaseOrdersPagedWithCountAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? supplierId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                var query = _context.PurchaseOrders.AsNoTracking()
                    .Include(po => po.Supplier)
                    .Include(po => po.Items)
                    .AsQueryable();

                // تطبيق الفلترة
                if (supplierId.HasValue)
                {
                    query = query.Where(po => po.SupplierId == supplierId.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(po => po.PurchaseDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(po => po.PurchaseDate < toDate.Value.AddDays(1));
                }

                var totalCount = await query.CountAsync();

                var data = await query
                    .OrderByDescending(po => po.PurchaseDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(po => new PurchaseOrderDto
                    {
                        Id = po.Id,
                        PurchaseDate = po.PurchaseDate,
                        SupplierId = po.SupplierId,
                        HasVAT = po.HasVAT, // ← إضافة هذا السطر
                        PurchaseNumber=po.PurchaseNumber,
                       
                        SupplierName = po.Supplier.Name,
                        Items = po.Items.Select(item => new PurchaseOrderItemDto
                        {
                            Id = item.Id,
                            Quantity = item.Quantity,
                            Price = item.Price,
                            Description = item.Description,
                            PurchaseOrderId = item.PurchaseOrderId
                        }).ToList(),
                        TotalAmount = po.Items.Sum(item => item.Quantity * item.Price),
                        VATAmount = po.HasVAT ? po.Items.Sum(item => item.Quantity * item.Price) * 0.05m : 0, // ← إضافة
                        TotalAmountWithVAT = po.HasVAT ?
                    po.Items.Sum(item => item.Quantity * item.Price) * 1.05m :
                    po.Items.Sum(item => item.Quantity * item.Price) // ← إضافة                    })
                    })
                    .ToListAsync();

                return (data, totalCount);
            }
            catch (Exception ex)
            {
                return (new List<PurchaseOrderDto>(), 0);
            }
        }

        // في الـ Repository
        public async Task<int> GetPurchaseOrdersCountAsync(
            int? supplierId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.PurchaseOrders.AsNoTracking().AsQueryable();

            // نفس الفلترة المستخدمة في GetAllPurchaseOrdersPagedAsync
            if (supplierId.HasValue)
            {
                query = query.Where(po => po.SupplierId == supplierId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(po => po.PurchaseDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(po => po.PurchaseDate < toDate.Value.AddDays(1));
            }

            return await query.CountAsync();
        }
        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _context.PurchaseOrders
                                 .Include(o => o.Supplier)
                                 .Include(o => o.Items)
                                 .OrderByDescending(o => o.PurchaseDate)
                                 .ToListAsync();
        }

        public async Task AddAsync(PurchaseOrder order)
        {
            await _context.PurchaseOrders.AddAsync(order);
        }

        public void Update(PurchaseOrder order)
        {
            _context.PurchaseOrders.Update(order);
        }

        public void Remove(PurchaseOrder order)
        {
            _context.PurchaseOrders.Remove(order);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<List<PurchaseOrderItem>> GetItemsByOrderIdAsync(int orderId)
        {
            return await _context.PurchaseOrderItems
                                 .Where(i => i.PurchaseOrderId == orderId)
                                 .ToListAsync();
        }
    }
}
