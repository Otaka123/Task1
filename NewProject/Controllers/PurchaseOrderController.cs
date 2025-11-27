using Application.Common;
using Application.Interfaces;
using Application.Services.OTP;
using Application.Services.PurchaseOrders;
using Domain.DTOS.Filters;
using Domain.DTOS.Purchase;
using Domain.Entities;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NewProject.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ISupplierRepository _supplierService;
        private readonly UserManager<Admin> _userManager;

        private readonly IPurchaseOrderNumberService _numberService;
        public PurchaseOrderController(
            IPurchaseOrderService purchaseOrderService,
            ISupplierRepository supplierService,
            UserManager<Admin> userManager,
            IPurchaseOrderNumberService numberService)
        {
            _userManager = userManager;
            _purchaseOrderService = purchaseOrderService;
            _supplierService = supplierService;
            _numberService = numberService;
        }

        // GET: PurchaseOrders/Index
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            int? supplierId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            try
            {
                if (fromDate.HasValue && toDate.HasValue &&
       fromDate > toDate)
                {
                    ModelState.AddModelError("", "تاريخ البداية لا يمكن أن يكون بعد تاريخ النهاية");
                    // يمكنك إعادة تعيين التواريخ أو التعامل مع الخطأ
                    toDate = fromDate;
                }
                // جلب بيانات الموردين للقائمة المنسدلة
                var suppliers = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", supplierId);

                // تعيين القيم في ViewBag للعرض في الفورم
                ViewBag.CurrentPage = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.SelectedSupplierId = supplierId;
                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

                // جلب أوامر الشراء
                var result = await _purchaseOrderService.GetAllPurchaseOrdersPagedAsync(
                    pageNumber, pageSize, supplierId, fromDate, toDate);

                if (!result.IsSuccess)
                {
                    TempData["Error"] = result.Message;
                    return View(new PagedResultN<PurchaseOrderDto> { Items = new List<PurchaseOrderDto>() });
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث حطأ اثناء جلب اوامر الشراء: {ex.Message}";
                return View(new PagedResultN<PurchaseOrderDto> { Items = new List<PurchaseOrderDto>() });
            }
        }

        //// GET: PurchaseOrders/Create
        //public async Task<IActionResult> Create()
        //{
        //    try
        //    {
        //        // جلب الموردين للقائمة المنسدلة
        //        var suppliers = await _supplierService.GetAllAsync();
        //        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name");
        //        var nextNumber = await _numberService.GetNextPurchaseOrderNumberAsync();
        //        ViewBag.NextPurchaseNumber = nextNumber;
        //        var model = new CreatePurchaseOrderDto
        //        {
        //            PurchaseDate = DateTime.Today,
        //            PurchaseNumber = nextNumber

        //        };

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error loading create form: {ex.Message}";
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        //// POST: PurchaseOrders/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(CreatePurchaseOrderDto model)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            var suppliers = await _supplierService.GetAllAsync();
        //            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
        //            ViewBag.NextPurchaseNumber = model.PurchaseNumber;
        //            ViewBag.Items = model.Items;

        //            return View(model);
        //        }
        //        bool numberExists = await _numberService.IsPurchaseNumberExistsAsync(model.PurchaseNumber);

        //        if (numberExists)
        //        {
        //            model.PurchaseNumber = await _numberService.GetNextPurchaseOrderNumberAsync();
        //            ModelState.AddModelError("", $"تم تحديث رقم أمر الشراء تلقائيًا إلى {model.PurchaseNumber} بسبب تعارض.");
        //        }


        //        var result = await _purchaseOrderService.CreatePurchaseOrderAsync(model);

        //        if (result.IsSuccess)
        //        {
        //            TempData["Success"] = "Purchase order created successfully!";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        // إعادة تعبئة القائمة المنسدلة في حالة الخطأ
        //        var suppliersList = await _supplierService.GetAllAsync();
        //        ViewBag.Suppliers = new SelectList(suppliersList, "Id", "Name", model.SupplierId);

        //        TempData["Error"] = result.Message;
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error creating purchase order: {ex.Message}";

        //        // إعادة تعبئة القائمة المنسدلة في حالة الخطأ
        //        var suppliers = await _supplierService.GetAllAsync();
        //        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);

        //        return View(model);
        //    }
        //}
        // GET: PurchaseOrders/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name");
                var nextNumber = await _numberService.GetNextPurchaseOrderNumberAsync();
                ViewBag.NextPurchaseNumber = nextNumber;
                ViewBag.IsEdit = false;

                var model = new CreatePurchaseOrderDto
                {
                    PurchaseDate = DateTime.Today,
                    PurchaseNumber = nextNumber
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"مشكله في صفحة الانشاء: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        // POST: PurchaseOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseOrderDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var suppliers = await _supplierService.GetAllAsync();
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
                    ViewBag.NextPurchaseNumber = model.PurchaseNumber;
                    ViewBag.Items = model.Items;
                    ViewBag.IsEdit = false;

                    return View(model);
                }

                bool numberExists = await _numberService.IsPurchaseNumberExistsAsync(model.PurchaseNumber);
                if (numberExists)
                {
                    model.PurchaseNumber = await _numberService.GetNextPurchaseOrderNumberAsync();
                    ModelState.AddModelError("", $"تم تحديث رقم أمر الشراء تلقائيًا إلى {model.PurchaseNumber} بسبب تعارض.");
                }

                var result = await _purchaseOrderService.CreatePurchaseOrderAsync(model);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "تم انشاء امر الشراء بنجاح !";
                    return RedirectToAction(nameof(Index));
                }

                var suppliersList = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliersList, "Id", "Name", model.SupplierId);
                ViewBag.IsEdit = false;

                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث مشكلة اثناء انشاء امر الشراء: {ex.Message}";

                var suppliers = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
                ViewBag.IsEdit = false;

                return View(model);
            }
        }

        // GET: PurchaseOrders/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(id);
                if (purchaseOrder == null)
                {
                    TempData["Error"] = "لم نجد امر الشراء ";
                    return RedirectToAction(nameof(Index));
                }

                var purchaseitems = await _purchaseOrderService.GetItemsByOrderIdAsync(id);
                var suppliers = await _supplierService.GetAllAsync();

                ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", purchaseOrder.SupplierId);
                ViewBag.IsEdit = true;



                var model = new UpdatePurchaseOrderDto
                {
                    Id = purchaseOrder.Id,
                    PurchaseNumber = purchaseOrder.PurchaseNumber,
                    PurchaseDate = purchaseOrder.PurchaseDate,
                    SupplierId = purchaseOrder.SupplierId,
                    HasVAT = purchaseOrder.HasVAT,
                    Items = purchaseOrder.Items?.Select(item => new CreatePurchaseOrderItemDto
                    {
                        Id = item.Id,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Description = item.Description,
                    }).ToList() ?? new List<CreatePurchaseOrderItemDto>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"مشكلة في صفحة تعديل امر الشراء: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        //POST: PurchaseOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePurchaseOrderDto model)
        {
            try
            {
                if (id != model.Id)
                {
                    TempData["Error"] = "مشكلة في معرف امر الشراء";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    var suppliers = await _supplierService.GetAllAsync();
                    ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
                    ViewBag.Items = model.Items;
                    ViewBag.IsEdit = true;

                    return View(model);
                }

                var result = await _purchaseOrderService.UpdatePurchaseOrderAsync(model);

                if (result.IsSuccess)
                {
                    TempData["Success"] = "تم تحديث امر الشراء بنجاح!";
                    return RedirectToAction(nameof(Index));
                }

                var suppliersList = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliersList, "Id", "Name", model.SupplierId);
                ViewBag.IsEdit = true;

                TempData["Error"] = result.Message;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث مشكلة اثناء تحديث امر الشراء: {ex.Message}";

                var suppliers = await _supplierService.GetAllAsync();
                ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
                ViewBag.IsEdit = true;

                return View(model);
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, UpdatePurchaseOrderDto model)
        //{
        //    try
        //    {
        //        if (id != model.Id)
        //        {
        //            TempData["Error"] = "Invalid purchase order ID";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        // تحقق يدوي من صحة البيانات قبل ModelState.IsValid
        //        if (model.Items != null)
        //        {
        //            for (int i = 0; i < model.Items.Count; i++)
        //            {
        //                var item = model.Items[i];
        //                if (item.Price <= 0)
        //                {
        //                    ModelState.AddModelError($"Items[{i}].Price", "السعر يجب أن يكون أكبر من الصفر");
        //                }
        //            }
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            // سجل الأخطاء للتdebug
        //            foreach (var error in ModelState)
        //            {
        //                if (error.Value.Errors.Count > 0)
        //                {
        //                    System.Diagnostics.Debug.WriteLine($"Error in {error.Key}: {error.Value.Errors.First().ErrorMessage}");
        //                }
        //            }

        //            var suppliers = await _supplierService.GetAllAsync();
        //            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
        //            ViewBag.Items = model.Items;
        //            ViewBag.IsEdit = true;

        //            return View(model);
        //        }

        //        var result = await _purchaseOrderService.UpdatePurchaseOrderAsync(model);

        //        if (result.IsSuccess)
        //        {
        //            TempData["Success"] = "Purchase order updated successfully!";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var suppliersList = await _supplierService.GetAllAsync();
        //        ViewBag.Suppliers = new SelectList(suppliersList, "Id", "Name", model.SupplierId);
        //        ViewBag.IsEdit = true;

        //        TempData["Error"] = result.Message;
        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"Error updating purchase order: {ex.Message}";

        //        var suppliers = await _supplierService.GetAllAsync();
        //        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", model.SupplierId);
        //        ViewBag.IsEdit = true;

        //        return View(model);
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> PrintPurchaseOrders(
            int? supplierId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // جلب جميع بيانات أوامر الشراء بدون pagination للطباعة
                var result = await _purchaseOrderService.GetAllPurchaseOrdersPagedAsync(
                    pageNumber: 1,
                    pageSize: 400, // حد أقصى للطباعة
                    supplierId,
                    fromDate,
                    toDate);

                if (!result.IsSuccess)
                {
                    TempData["Error"] = "حدث مشكلة اثناء جلب اوامر الشراء";
                    return View("PrintPurchaseOrders", new PagedResultN<PurchaseOrderDto> { Items = new List<PurchaseOrderDto>() });
                }

                // التحقق من أن العدد الإجمالي لا يتجاوز 400
                if (result.Data.TotalCount > 400)
                {
                    var limitedItems = result.Data.Items.Take(400).ToList();
                    var limitedResult = new PagedResultN<PurchaseOrderDto>
                    {
                        Items = limitedItems,
                        TotalCount = limitedItems.Count,
                        PageNumber = 1,
                        PageSize = 400
                    };

                    ViewData["Title"] = "طباعة تقرير أوامر الشراء (مقيد إلى 400 سجل)";
                    ViewData["PrintDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                    ViewData["Criteria"] = $"المورد: {supplierId} | من: {fromDate:yyyy-MM-dd} | إلى: {toDate:yyyy-MM-dd}";

                    return View("PrintPurchaseOrders", limitedResult);
                }

                ViewData["Title"] = "طباعة تقرير أوامر الشراء";
                ViewData["PrintDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
                ViewData["Criteria"] = $"المورد: {supplierId} | من: {fromDate:yyyy-MM-dd} | إلى: {toDate:yyyy-MM-dd}";

                return View("PrintPurchaseOrders", result.Data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء تحضير البيانات للطباعة";
                return View("PrintPurchaseOrders", new PagedResultN<PurchaseOrderDto> { Items = new List<PurchaseOrderDto>() });
            }
        }
        // GET: PurchaseOrders/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var result = await _purchaseOrderService.GetByIdWithItemssAsync(id);
                var User = await _userManager.FindByIdAsync(result.AuthorizedBy);
                if (result.Items == null)
                {
                    TempData["Error"] = "مشكلة في صفحة التعديل";
                    return RedirectToAction(nameof(Index));
                }

                // إضافة بيانات التوقيع
                var signatureRepository = HttpContext.RequestServices.GetService<ISignatureRepository>();
                if (signatureRepository != null)
                {
                    var signature = await signatureRepository.GetByPurchaseOrderIdAsync(id);
                    if (signature != null)
                    {
                        result.IsSigned = !string.IsNullOrEmpty(signature.SignaturePath);
                        result.SignaturePath = signature.SignaturePath;
                        result.SignedDate = signature.SignedDate;
                        result.AuthorizedBy = User.FirstName + " " + User.LastName;
                        result.Designation = User.JopTitle;

                    }
                }

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading purchase order details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // إضافة تحقق إضافي
                if (id <= 0)
                {
                    TempData["Error"] = "معرف غير صحيح";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _purchaseOrderService.DeleteAsync(id);

                if (result)
                {
                    TempData["Success"] = "تم حذف أمر الشراء بنجاح!";
                }
                else
                {
                    TempData["Error"] = "فشل في حذف أمر الشراء - قد يكون مرتبطاً ببيانات أخرى";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ أثناء الحذف: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


    }
}
