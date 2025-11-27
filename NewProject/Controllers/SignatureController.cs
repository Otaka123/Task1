using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewProject.VM;
using System.Security.Claims;

namespace NewProject.Controllers
{
    public class SignatureController : Controller
    {
        private readonly IOTPService _otpService;
        private readonly AppDbContext _context;
        private readonly ISignatureRepository _signatureRepository;
        private readonly UserManager<Admin> _userManager;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public SignatureController(
            IOTPService otpService,
            ISignatureRepository signatureRepository,
            UserManager<Admin> userManager,
            AppDbContext context,
            IPurchaseOrderService purchaseOrderService)
        {
            _context = context;
            _otpService = otpService;
            _signatureRepository = signatureRepository;
            _userManager = userManager;
            _purchaseOrderService = purchaseOrderService;
        }
        [HttpGet]
        public async Task<JsonResult> GetUserSignature()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, error = "يجب تسجيل الدخول أولاً" });
                }
                if (user != null)
                {
                    await _context.Entry(user).Reference(u => u.Signature).LoadAsync();
                }

                if (user.Signature == null || string.IsNullOrEmpty(user.Signature.SignaturePath))
                {
                    return Json(new { success = false, error = "ليس لديك توقيع مسجل" });
                }

                return Json(new
                {
                    success = true,
                    signaturePath = user.Signature.SignaturePath
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"حدث خطأ: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RequestOTPAjax([FromBody] OTPRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, error = "يجب تسجيل الدخول أولاً" });
                }

                var result = await _otpService.GenerateAndSendOTPAsync(
                    user.Id,
                    user.phone,
                    request.PurchaseOrderId
                );

                if (!result.Success)
                {
                    return Json(new { success = false, error = result.ErrorMessage });
                }

                return Json(new
                {
                    success = true,
                    message = result.Message,
                    expiryMinutes = result.ExpiryMinutes
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"حدث خطأ: {ex.Message}" });
            }
        }

        public class OTPRequest
        {
            public int PurchaseOrderId { get; set; }
        }

        // POST: التحقق من OTP والتوقيع عبر AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> VerifyAndSignAjax([FromBody] VerifyOTPAjaxViewModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // التحقق من OTP
                var verificationResult = await _otpService.VerifyOTPAsync(
                    model.PurchaseOrderId,
                    model.OTP,
                    userId
                );

                if (!verificationResult.IsValid)
                {
                    return Json(new { success = false, error = verificationResult.ErrorMessage });
                }

                // الحصول على التوقيع
                var signature = await _signatureRepository.GetByPurchaseOrderIdAsync(model.PurchaseOrderId);
                if (signature == null)
                {
                    return Json(new { success = false, error = "لم يتم العثور على التوقيع" });
                }

                var user = await _userManager.GetUserAsync(User);

                string signaturePath = signature.SignaturePath;

                // تحديث التوقيع
                //signature.SignaturePath = signaturePath;
                //signature.SignedDate = DateTime.UtcNow;

                // مسح OTP بعد الاستخدام الناجح
                signature.OTPCode = null;
                signature.OTPExpiry = null;

                _signatureRepository.Update(signature);
                await _signatureRepository.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "تم التوقيع بنجاح",
                    signaturePath = signaturePath
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"حدث خطأ أثناء التوقيع: {ex.Message}" });
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<JsonResult> VerifyAndSignAjax([FromBody] VerifyOTPAjaxViewModel model)
        //{
        //    try
        //    {
        //        // التحقق من OTP
        //        var verificationResult = await _otpService.VerifyOTPAsync(
        //            model.PurchaseOrderId,
        //            model.OTP
        //        );

        //        if (!verificationResult.IsValid)
        //        {
        //            return Json(new { success = false, error = verificationResult.ErrorMessage });
        //        }

        //        var user = await _userManager.GetUserAsync(User);

        //        // استخدام التوقيع المخزن للمستخدم (وليس حفظ صورة جديدة)
        //        var userSignature = await _signatureRepository.GetByAdminIdAsync(user.Id);
        //        if (userSignature == null)
        //        {
        //            return Json(new { success = false, error = "ليس لديك توقيع مسجل" });
        //        }

        //        // مسح OTP بعد الاستخدام الناجح
        //        userSignature.OTPCode = null;
        //        userSignature.OTPExpiry = null;
        //        userSignature.SignedDate = DateTime.UtcNow;

        //        _signatureRepository.Update(userSignature);
        //        await _signatureRepository.SaveChangesAsync();

        //        return Json(new
        //        {
        //            success = true,
        //            message = "تم التوقيع بنجاح",
        //            signaturePath = userSignature.SignaturePath // استخدام المسار المخزن
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = $"حدث خطأ أثناء التوقيع: {ex.Message}" });
        //    }
        //}
        // GET: طلب OTP (عرض النموذج)
        [HttpGet]
        public async Task<IActionResult> RequestOTP(int purchaseOrderId)
        {
            try
            {
                // التحقق من وجود أمر الشراء
                var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    TempData["Error"] = "لم يتم العثور على أمر الشراء";
                    return RedirectToAction("Details", "PurchaseOrder", new { id = purchaseOrderId });
                }

                var model = new RequestOTPViewModel
                {
                    PurchaseOrderId = purchaseOrderId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
                return RedirectToAction("Details", "PurchaseOrder", new { id = purchaseOrderId });
            }
        }

        // POST: طلب OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestOTP(RequestOTPViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "يجب تسجيل الدخول أولاً";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _otpService.GenerateAndSendOTPAsync(
                    user.Id,
                    user.phone,
                    model.PurchaseOrderId
                );

                if (!result.Success)
                {
                    TempData["Error"] = result.ErrorMessage;
                    return View(model);
                }

                TempData["Success"] = $"{result.Message} (صالح لمدة {result.ExpiryMinutes} دقيقة)";
                return RedirectToAction("VerifyAndSign", new { purchaseOrderId = model.PurchaseOrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ أثناء طلب رمز التحقق: {ex.Message}";
                return View(model);
            }
        }

        // GET: التحقق من OTP والتوقيع (عرض النموذج)
        [HttpGet]
        public async Task<IActionResult> VerifyAndSign(int purchaseOrderId)
        {
            try
            {
                // التحقق من وجود أمر الشراء
                var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    TempData["Error"] = "لم يتم العثور على أمر الشراء";
                    return RedirectToAction("Index", "PurchaseOrder");
                }

                var model = new VerifyOTPViewModel
                {
                    PurchaseOrderId = purchaseOrderId
                };

                ViewBag.PurchaseOrder = purchaseOrder;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"حدث خطأ: {ex.Message}";
                return RedirectToAction("Details", "PurchaseOrder", new { id = purchaseOrderId });
            }
        }

        // POST: التحقق من OTP والتوقيع
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAndSign(VerifyOTPViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // إعادة تعبئة بيانات أمر الشراء في حالة الخطأ
                    var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(model.PurchaseOrderId);
                    ViewBag.PurchaseOrder = purchaseOrder;
                    return View(model);
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // التحقق من OTP
                var verificationResult = await _otpService.VerifyOTPAsync(
                    model.PurchaseOrderId,
                    model.OTP,
                    userId
                );

                if (!verificationResult.IsValid)
                {
                    TempData["Error"] = verificationResult.ErrorMessage;

                    // إعادة تعبئة بيانات أمر الشراء
                    var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(model.PurchaseOrderId);
                    ViewBag.PurchaseOrder = purchaseOrder;

                    return View(model);
                }

                // الحصول على التوقيع
                var signature = await _signatureRepository.GetByPurchaseOrderIdAsync(model.PurchaseOrderId);
                if (signature == null)
                {
                    TempData["Error"] = "لم يتم العثور على التوقيع";

                    var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(model.PurchaseOrderId);
                    ViewBag.PurchaseOrder = purchaseOrder;

                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);

                // حفظ صورة التوقيع
                string signaturePath = await SaveSignatureImage(model.SignatureImage, user.Id);

                // تحديث التوقيع
                signature.SignaturePath = signaturePath;
                signature.SignedDate = DateTime.UtcNow;

                // مسح OTP بعد الاستخدام الناجح
                signature.OTPCode = null;
                signature.OTPExpiry = null;

                _signatureRepository.Update(signature);
                await _signatureRepository.SaveChangesAsync();

                TempData["Success"] = "تم التوقيع بنجاح";
                return RedirectToAction("Details", "PurchaseOrder", new { id = model.PurchaseOrderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "حدث خطأ أثناء التوقيع";

                // إعادة تعبئة بيانات أمر الشراء في حالة الخطأ
                var purchaseOrder = await _purchaseOrderService.GetByIdWithItemsAsync(model.PurchaseOrderId);
                ViewBag.PurchaseOrder = purchaseOrder;

                return View(model);
            }
        }

        private async Task<string> SaveSignatureImage(string base64Image, string userId)
        {
            try
            {
                // تحويل base64 إلى صورة وحفظها
                var imageData = base64Image.Split(',')[1]; // إزالة data:image/png;base64,
                var bytes = Convert.FromBase64String(imageData);

                var fileName = $"signature_{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                var filePath = Path.Combine("wwwroot", "signatures", fileName);

                // التأكد من وجود المجلد
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return $"/signatures/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception("فشل في حفظ صورة التوقيع");
            }
        }
    }
}
