using Application.Interfaces;
using Application.Services.SMS;
using Domain.DTOS.OTP;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.OTP
{
    public class OTPService : IOTPService
    {
        private readonly ISignatureRepository _signatureRepository;
        private readonly IPurchaseOrderService  _purchaseOrderRepository;
        private readonly ISmsService _smsService;
        private readonly IConfiguration _configuration;

        public OTPService(
            ISignatureRepository signatureRepository,
IPurchaseOrderService purchasorder,
ISmsService smsService,
            IConfiguration configuration)
        {
            _signatureRepository = signatureRepository;
            _purchaseOrderRepository = purchasorder;
            _smsService = smsService;
            _configuration = configuration;
        }
        public async Task<OTPGenerationResult> GenerateAndSendOTPAsync(string userId, string phoneNumber, int purchaseOrderId)
        {
            try
            {
                // البحث عن أمر الشراء أولاً مع تضمين الـ Signature
                var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    return new OTPGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "لم يتم العثور على أمر الشراء"
                    };
                }

                // جلب التوقيع الموجود للمستخدم (وليس إنشاء جديد)
                var userSignature = await _signatureRepository.GetByAdminIdAsync(userId);
                if (userSignature == null)
                {
                    return new OTPGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "ليس لديك توقيع مسجل. يرجى التواصل مع المسؤول."
                    };
                }

                // إذا كان هناك توقيع مسبق مرتبط بأمر الشراء و OTP صالح
                if (purchaseOrder.Signature != null && purchaseOrder.Signature.IsOTPValid())
                {
                    return new OTPGenerationResult
                    {
                        Success = true,
                        OTP = purchaseOrder.Signature.OTPCode ?? string.Empty,
                        ExpiryMinutes = Math.Max((int)(purchaseOrder.Signature.OTPExpiry - DateTime.UtcNow).Value.TotalMinutes, 10)
                    };
                }

                // توليد OTP
                var random = new Random();
                //var otp = random.Next(1000, 9999).ToString();
                var otp = "1111";
                var expiryMinutes = 10;
                var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

                // تحديث التوقيع الحالي للمستخدم (وليس إنشاء جديد)
                userSignature.OTPCode = otp;
                userSignature.OTPExpiry = expiryTime;


                _signatureRepository.Update(userSignature);

                var saveResult = await _signatureRepository.SaveChangesAsync();
                if (!saveResult)
                {
                    return new OTPGenerationResult
                    {
                        Success = false,
                        ErrorMessage = "فشل في حفظ كود OTP"
                    };
                }

                // إرسال OTP
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    var smsSent = await SendSmsOTPAsync(phoneNumber, otp);
                    if (!smsSent)
                    {
                        return new OTPGenerationResult
                        {
                            Success = false,
                            ErrorMessage = "فشل في إرسال كود OTP عبر SMS"
                        };
                    }
                }

                return new OTPGenerationResult
                {
                    Success = true,
                    OTP = otp,
                    ExpiryMinutes = expiryMinutes,
                    Message = "تم إرسال كود OTP بنجاح"
                };
            }
            catch (Exception ex)
            {
                return new OTPGenerationResult
                {
                    Success = false,
                    ErrorMessage = $"حدث خطأ غير متوقع أثناء توليد OTP: {ex.Message}"
                };
            }
        }
        public async Task<OTPVerificationResult> VerifyOTPAsync(int purchaseOrderId, string otp, string userid)
        {
            try
            {
                // البحث عن أمر الشراء والتوقيع
                var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(purchaseOrderId);
                if (purchaseOrder == null)
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        ErrorMessage = "لم يتم العثور على أمر الشراء"
                    };
                }

                var userSignature = await _signatureRepository.GetByAdminIdAsync(userid);
                if (userSignature == null)
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        ErrorMessage = "ليس لديك توقيع مسجل"
                    };
                }


                // التحقق من OTP
                if (string.IsNullOrEmpty(userSignature.OTPCode) || userSignature.OTPCode != otp)
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        ErrorMessage = "رمز التحقق غير صحيح"
                    };
                }

                // التحقق من انتهاء الصلاحية
                if (!userSignature.OTPExpiry.HasValue || userSignature.OTPExpiry < DateTime.UtcNow)
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        ErrorMessage = "رمز التحقق منتهي الصلاحية"
                    };
                }

                // تحديث بيانات التوقيع
                userSignature.SignedDate = DateTime.UtcNow;

                // ربط التوقيع الحالي بأمر الشراء
                if (purchaseOrder.Signature == null || purchaseOrder.Signature.Id != userSignature.Id)
                {
                    purchaseOrder.Signature = userSignature;

                    purchaseOrder.SignatureId = userSignature.Id;
                    await _purchaseOrderRepository.UpdateAsync(purchaseOrder);
                }

                // مسح OTP بعد الاستخدام الناجح
                userSignature.OTPCode = null;
                userSignature.OTPExpiry = null;

                _signatureRepository.Update(userSignature);

                var saveResult = await _signatureRepository.SaveChangesAsync();
                if (!saveResult)
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        ErrorMessage = "فشل في تحديث بيانات التوقيع"
                    };
                }

                return new OTPVerificationResult
                {
                    IsValid = true,
                    Signature = userSignature
                };
            }
            catch (Exception ex)
            {
                return new OTPVerificationResult
                {
                    IsValid = false,
                    ErrorMessage = $"حدث خطأ أثناء التحقق من OTP: {ex.Message}"
                };
            }
        }

        // دالة مساعدة لإخفاء أرقام الهاتف للأمان
        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length < 4)
                return phoneNumber;

            var visiblePart = phoneNumber.Substring(phoneNumber.Length - 4);
            return $"******{visiblePart}";
        }

        //private async Task<bool> SendEmailOTPAsync(string email, string otp, int expiryMinutes)
        //{
        //    try
        //    {
        //        var emailSubject = "كود التحقق لتوقيع أمر الشراء";
        //        var emailBody = $@"
        //            <div style='font-family: Arial, sans-serif; direction: rtl;'>
        //                <h2 style='color: #2c3e50;'>كود التحقق لتوقيع أمر الشراء</h2>
        //                <p>عزيزي المستخدم،</p>
        //                <p>كود التحقق الخاص بك هو: <strong style='font-size: 24px; color: #e74c3c;'>{otp}</strong></p>
        //                <p>هذا الكود صالح لمدة {expiryMinutes} دقائق فقط.</p>
        //                <p>إذا لم تطلب هذا الكود، يرجى تجاهل هذه الرسالة.</p>
        //                <hr style='margin: 20px 0;'>
        //                <p style='color: #7f8c8d; font-size: 12px;'>هذه رسالة آلية، يرجى عدم الرد عليها.</p>
        //            </div>";

        //        return await _emailService.SendEmailAsync(email, emailSubject, emailBody);
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        private async Task<bool> SendSmsOTPAsync(string phoneNumber, string otp)
        {
            try
            {
                var result = await _smsService.SendSMSAsync(phoneNumber, otp);
                return result.Success;
            }
            catch
            {
                return false;
            }
        }
     
        public async Task<bool> ConsumeOTPAsync(int purchaseOrderId, string otp)
        {
            try
            {
                var signature = await _signatureRepository.GetByPurchaseOrderIdAsync(purchaseOrderId);
                if (signature == null || signature.OTPCode != otp)
                    return false;

                // مسح OTP بعد الاستخدام الناجح
                signature.OTPCode = null;
                signature.OTPExpiry = null;
                _signatureRepository.Update(signature);

                return await _signatureRepository.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsOTPValidAsync(int purchaseOrderId, string otp,string userid)
        {
            var result = await VerifyOTPAsync(purchaseOrderId, otp, userid);
            return result.IsValid;
        }

        public async Task<string> GetStoredOTPAsync(int purchaseOrderId)
        {
            var signature = await _signatureRepository.GetByPurchaseOrderIdAsync(purchaseOrderId);
            return signature?.OTPCode ?? string.Empty;
        }
    }
}
