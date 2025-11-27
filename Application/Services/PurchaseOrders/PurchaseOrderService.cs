using Application.Common;
using Application.Interfaces;
using Domain.DTOS.Filters;
using Domain.DTOS.OTP;
using Domain.DTOS.Purchase;
using Domain.Entities;
using Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.PurchaseOrders
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _repository;
        private readonly IPurchaseOrderNumberService _NoService;

        public PurchaseOrderService(IPurchaseOrderRepository repository, IPurchaseOrderNumberService noService)
        {
            _repository = repository;
            _NoService = noService;
        }

        public async Task<PurchaseOrder?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
        // دالة التحويل من Entity إلى DTO
      

   
        private PurchaseOrderDto MapToDto(PurchaseOrder purchaseOrder)
        {
            var dto = new PurchaseOrderDto
            {
                Id = purchaseOrder.Id,
                PurchaseDate = purchaseOrder.PurchaseDate,
                SupplierId = purchaseOrder.SupplierId,
                PurchaseNumber = purchaseOrder.PurchaseNumber,
                HasVAT = purchaseOrder.HasVAT,
                SupplierName = purchaseOrder.Supplier?.Name ?? string.Empty,
                Items = purchaseOrder.Items?.Select(item => new PurchaseOrderItemDto
                {
                    Id = item.Id,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Description = item.Description
                }).ToList() ?? new List<PurchaseOrderItemDto>()
            };

            // حساب المبالغ
            CalculateAmounts(dto);

            return dto;
        }
        public async Task<bool> SignPurchaseOrderAsync(int purchaseOrderId, string adminId, string signaturePath, string otpCode = null)
        {
            try
            {
                // التحقق من وجود أمر الشراء
                var purchaseOrderExists = await _repository.GetByIdAsync(purchaseOrderId);
                if (purchaseOrderExists==null)
                {
                    return false;
                }

                // التحقق إذا كان هناك توقيع مسبق
                var isAlreadySigned = await _repository.IsPurchaseOrderSignedAsync(purchaseOrderId);
                if (isAlreadySigned)
                {
                    return false; // أو يمكنك تحديث التوقيع الحالي بدلاً من ذلك
                }

                var signature = new Signature
                {
                    AdminId = adminId,
                    SignaturePath = signaturePath,
                    OTPCode = otpCode,
                    OTPExpiry = otpCode != null ? DateTime.UtcNow.AddMinutes(10) : null
                };

                await _repository.AddSignatureAsync(signature);
                return await _repository.SaveAsync()>0;
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ
                Console.WriteLine($"Error signing purchase order: {ex.Message}");
                return false;
            }
        }


        // دالة حساب المبالغ
        private void CalculateAmounts(PurchaseOrderDto dto)
        {
            dto.TotalAmount = dto.Items.Sum(item => item.Quantity * item.Price);

            if (dto.HasVAT)
            {
                dto.VATAmount = dto.TotalAmount * 0.05m; // 5% VAT
                dto.TotalAmountWithVAT = dto.TotalAmount + dto.VATAmount;
            }
            else
            {
                dto.VATAmount = 0;
                dto.TotalAmountWithVAT = dto.TotalAmount;
            }
        }
        //public async Task<PurchaseOrder?> GetByIdWithitemsAsync(int id)
        //{
        //    return await _repository.GetByIdWithItemsAsync(id);
        //}

        public async Task<ViewPurchaseOrderDto?> GetByIdWithItemssAsync(int id)
        {
            var purchaseOrder = await _repository.GetByIdWithItemsAsync(id);
            if (purchaseOrder == null)
                return null;

            return MapToViewDto(purchaseOrder);
        }

        private ViewPurchaseOrderDto MapToViewDto(PurchaseOrder po)
        {
            var dto = new ViewPurchaseOrderDto
            {
                Id = po.Id,
                PurchaseOrderCode = po.PurchaseNumber.ToString(),
                Date = po.PurchaseDate,
                SupplierName = po.Supplier?.Name ?? string.Empty,
                Items = po.Items.Select(i => new PurchaseOrderItemDto
                {
                    Id = i.Id,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Price = i.Price,
                }).ToList(),
                TotalAmount = po.Items.Sum(i => i.Quantity * i.Price),
                VATAmount = po.HasVAT ? po.Items.Sum(i => i.Quantity * i.Price) * 0.14m : 0, // مثال: 14% ضريبة
                GrandTotal = po.Items.Sum(i => i.Quantity * i.Price) + (po.HasVAT ? po.Items.Sum(i => i.Quantity * i.Price) * 0.14m : 0),
                TotalAmountInWords = NumberToWords(po.Items.Sum(i => i.Quantity * i.Price)), // تحتاج دالة تحويل للأرقام لكلمات
                HasVAT = po.HasVAT,
                AuthorizedBy = po.Signature?.AdminId,
                //Designation = po.Signature?.Designation,
                IsSigned = po.Signature != null,
                SignaturePath = po.Signature?.SignaturePath,
                SignedDate = po.Signature?.SignedDate
            };

            return dto;
        }

        // مثال دالة بسيطة لتحويل الأرقام لكلمات
        private string NumberToWords(decimal number)
        {
            // هنا يمكنك استخدام مكتبة جاهزة أو كتابة تحويلك الخاص
            return number.ToString("N2"); // مؤقتا مجرد تحويل ل string
        }

        public async Task<PurchaseOrderDto?> GetByIdWithItemsAsync(int id)
        {
            var purchaseOrder = await _repository.GetByIdWithItemsAsync(id);
            return purchaseOrder == null ? null : MapToDto(purchaseOrder);
        }
        public async Task<List<PurchaseOrder>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PurchaseOrder> CreateAsync(PurchaseOrder order)
        {
            await _repository.AddAsync(order);
            await _repository.SaveAsync();
            return order;
        }
        public async Task<RequestResponse<PurchaseOrderDto>> CreatePurchaseOrderAsync(CreatePurchaseOrderDto createDto)
        {
            try
            {
                if (createDto == null)
                    return RequestResponse<PurchaseOrderDto>.BadRequest("Purchase order data is required");

                if (!createDto.Items.Any())
                    return RequestResponse<PurchaseOrderDto>.BadRequest("At least one item is required");

                var nextNumber = await _NoService.GetNextPurchaseOrderNumberAsync();

                // إنشاء كائن PurchaseOrder
                var purchaseOrder = new PurchaseOrder
                {
                    PurchaseDate = createDto.PurchaseDate,
                    SupplierId = createDto.SupplierId,
                    PurchaseNumber = nextNumber,
                    HasVAT = createDto.HasVAT, // ← إضافة هذا السطر
                    Items = createDto.Items.Select(item => new PurchaseOrderItem
                    {
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description
                    }).ToList()
                };

                // استدعاء دالة الـ Repository
                var createdOrder = await _repository.CreateWithItemsAsync(purchaseOrder);

                // حساب الإجماليات
                var totalAmount = createdOrder.Items.Sum(item => item.Quantity * item.Price);
                var vatAmount = createdOrder.HasVAT ? totalAmount * 0.05m : 0;
                var totalAmountWithVAT = totalAmount + vatAmount;

                // تحويل إلى DTO
                var resultDto = new PurchaseOrderDto
                {
                    Id = createdOrder.Id,
                    PurchaseDate = createdOrder.PurchaseDate,
                    PurchaseNumber = createdOrder.PurchaseNumber,
                    HasVAT = createdOrder.HasVAT, // ← إضافة هذا السطر
                    SupplierId = createdOrder.SupplierId,
                    SupplierName = createdOrder.Supplier?.Name,
                    Items = createdOrder.Items.Select(item => new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description,
                        PurchaseOrderId = item.PurchaseOrderId
                    }).ToList(),
                    TotalAmount = totalAmount,
                    VATAmount = vatAmount, // ← إضافة
                    TotalAmountWithVAT = totalAmountWithVAT // ← إضافة
                };

                return RequestResponse<PurchaseOrderDto>.Success(resultDto, "Purchase order created successfully");
            }
            catch (Exception ex)
            {
                return RequestResponse<PurchaseOrderDto>.InternalServerError($"Error creating purchase order: {ex.Message}");
            }
        }
        public async Task<RequestResponse<PurchaseOrderDto>> UpdatePurchaseOrderAsync(UpdatePurchaseOrderDto updateDto)
        {
            try
            {
                if (updateDto == null)
                    return RequestResponse<PurchaseOrderDto>.BadRequest("Purchase order data is required");

                if (updateDto.Id <= 0)
                    return RequestResponse<PurchaseOrderDto>.BadRequest("Valid purchase order ID is required");

                if (!updateDto.Items.Any())
                    return RequestResponse<PurchaseOrderDto>.BadRequest("At least one item is required");

                // تحويل DTO إلى كائن PurchaseOrder
                var purchaseOrder = new PurchaseOrder
                {
                    Id = updateDto.Id,
                    PurchaseDate = updateDto.PurchaseDate,
                    SupplierId = updateDto.SupplierId,
                    HasVAT = updateDto.HasVAT,
                    Items = updateDto.Items.Select(item => new PurchaseOrderItem
                    {
                        // لا نضع Id هنا لأننا سنضيفها كعناصر جديدة
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description
                        // PurchaseOrderId سيتم تعيينه تلقائياً في الـ Repository
                    }).ToList()
                };

                // استدعاء دالة الـ Repository المحدثة
                var updatedOrder = await _repository.UpdateWithItemsAsync(purchaseOrder);

                // حساب الإجماليات
                var totalAmount = updatedOrder.Items.Sum(item => item.Quantity * item.Price);
                var vatAmount = updatedOrder.HasVAT ? totalAmount * 0.05m : 0;
                var totalAmountWithVAT = totalAmount + vatAmount;

                // تحويل إلى DTO
                var resultDto = new PurchaseOrderDto
                {
                    Id = updatedOrder.Id,
                    PurchaseDate = updatedOrder.PurchaseDate,
                    PurchaseNumber = updatedOrder.PurchaseNumber,
                    HasVAT = updatedOrder.HasVAT,
                    SupplierId = updatedOrder.SupplierId,
                    SupplierName = updatedOrder.Supplier?.Name,
                    Items = updatedOrder.Items.Select(item => new PurchaseOrderItemDto
                    {
                        Id = item.Id,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description,
                        PurchaseOrderId = item.PurchaseOrderId
                    }).ToList(),
                    TotalAmount = totalAmount,
                    VATAmount = vatAmount,
                    TotalAmountWithVAT = totalAmountWithVAT
                };

                return RequestResponse<PurchaseOrderDto>.Success(resultDto, "Purchase order updated successfully");
            }
            catch (Exception ex)
            {
                return RequestResponse<PurchaseOrderDto>.InternalServerError($"Error updating purchase order: {ex.Message}");
            }
        }
        public async Task<RequestResponse<PagedResultN<PurchaseOrderDto>>> GetAllPurchaseOrdersPagedAsync(
    int pageNumber = 1,
    int pageSize = 10,
    int? supplierId = null,
    DateTime? fromDate = null,
    DateTime? toDate = null)
        {
            try
            {
                // استدعاء الدالة المعدلة من الـ Repository
                var (data, totalCount) = await _repository.GetAllPurchaseOrdersPagedWithCountAsync(
                    pageNumber, pageSize, supplierId, fromDate, toDate);

                // إنشاء وملء PagedResultN
                var pagedResult = new PagedResultN<PurchaseOrderDto>
                {
                    Items = data,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                return RequestResponse<PagedResultN<PurchaseOrderDto>>.Success(
                    pagedResult,
                    data.Count > 0 ? "Purchase orders retrieved successfully" : "No purchase orders found");
            }
            catch (Exception ex)
            {
                return RequestResponse<PagedResultN<PurchaseOrderDto>>.InternalServerError(
                    $"Error retrieving purchase orders: {ex.Message}");
            }
        }
        public async Task<PurchaseOrder> UpdateAsync(PurchaseOrder order)
        {
            var existing = await _repository.GetByIdAsync(order.Id);
            if (existing == null)
                throw new KeyNotFoundException("Purchase order not found");

            existing.PurchaseDate = order.PurchaseDate;
            existing.PurchaseNumber = order.PurchaseNumber;
            existing.HasVAT = order.HasVAT; // ← إضافة هذا السطر
            existing.SupplierId = order.SupplierId;
            existing.Items = order.Items;
            existing.Signature = order.Signature;
            existing.SignatureId = order.SignatureId;
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

        public async Task<List<PurchaseOrderItem>> GetItemsByOrderIdAsync(int orderId)
        {
            return await _repository.GetItemsByOrderIdAsync(orderId);
        }
    }
}
