using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PurchaseOrderItems
{
    public class PurchaseOrderItemService : IPurchaseOrderItemService
    {
        private readonly IPurchaseOrderItemRepository _repository;

        public PurchaseOrderItemService(IPurchaseOrderItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<PurchaseOrderItem?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<PurchaseOrderItem>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<List<PurchaseOrderItem>> GetByOrderIdAsync(int orderId)
        {
            return await _repository.GetByOrderIdAsync(orderId);
        }

        public async Task<PurchaseOrderItem> CreateAsync(PurchaseOrderItem item)
        {
            await _repository.AddAsync(item);
            await _repository.SaveAsync();
            return item;
        }

        public async Task<PurchaseOrderItem> UpdateAsync(PurchaseOrderItem item)
        {
            var existing = await _repository.GetByIdAsync(item.Id);
            if (existing == null)
                throw new KeyNotFoundException("Purchase order item not found");

            existing.Quantity = item.Quantity;
            existing.Price = item.Price;
            existing.Description = item.Description;
            existing.PurchaseOrderId = item.PurchaseOrderId;

            _repository.Update(existing);
            await _repository.SaveAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            _repository.Remove(existing);
            await _repository.SaveAsync();
            return true;
        }
    }
}
