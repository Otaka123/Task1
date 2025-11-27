using Application.Interfaces;
using Domain.DTOS.OTP;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Signatures
{
    public class SignatureService : ISignatureService
    {
        private readonly AppDbContext _context;
        private readonly ISmsService _smsService;
        private readonly Random _random = new();
        private readonly UserManager<Admin> _userManager;

        public SignatureService(AppDbContext context, ISmsService smsService,UserManager<Admin> userManager)
        {
            _userManager=userManager;
            _context = context;
            _smsService = smsService;
        }
        public async Task<ServiceResponse<string>> SendSignatureOTPAsync(int signatureId, string adminId, string mobileNumber)
        {
            try
            {
                // استخدام DbContext مباشرة مع Include
                var admin = await _context.Users
                    .Include(u => u.Signature)  // هذا هو المفتاح!
                    .FirstOrDefaultAsync(u => u.Id == adminId);

                if (admin == null)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "المستخدم غير موجود",
                        Data = null
                    };
                }

                if (admin.Signature == null)
                {
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "التوقيع غير موجود أو غير مسموح بالوصول",
                        Data = null
                    };
                }

                // التحقق من أن رقم الهاتف متوفر
                if (string.IsNullOrEmpty(mobileNumber))
                {
                    // استخدام رقم الهاتف من Admin
                    if (!string.IsNullOrEmpty(admin.phone))
                    {
                        mobileNumber = admin.phone;
                    }
                    else
                    {
                        return new ServiceResponse<string>
                        {
                            Success = false,
                            Message = "رقم الهاتف غير متوفر",
                            Data = null
                        };
                    }
                }

                // التحقق من وجود OTP صالح مسبقاً
                if (admin.Signature.IsOTPValid() && !string.IsNullOrEmpty(admin.Signature.OTPCode))
                {
                    return new ServiceResponse<string>
                    {
                        Success = true,
                        Message = "تم إرسال رمز التحقق مسبقاً",
                        Data = admin.Signature.OTPCode
                    };
                }

                // Generate 6-digit OTP
                string otp = _random.Next(100000, 999999).ToString();

                // تحديث التوقيع بـ OTP
                admin.Signature.OTPCode = otp;
                admin.Signature.OTPExpiry = DateTime.UtcNow.AddMinutes(5); // OTP صالح لمدة 5 دقائق

                _context.Signatures.Update(admin.Signature);
                await _context.SaveChangesAsync();

                // إرسال SMS بالـ OTP
                string message = $"رمز التحقق لتوقيع المستند: {otp}. صالح لمدة 5 دقائق";

                var smsResult = await _smsService.SendSMSAsync(mobileNumber, message);

                if (smsResult.Success)
                {
                    return new ServiceResponse<string>
                    {
                        Success = true,
                        Message = "تم إرسال رمز التحقق بنجاح",
                        Data = otp
                    };
                }

                // إذا فشل إرسال SMS، نمسح OTP
                admin.Signature.OTPCode = null;
                admin.Signature.OTPExpiry = null;
                await _context.SaveChangesAsync();

                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"فشل في إرسال رمز التحقق: {smsResult.Message}",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"حدث خطأ أثناء إرسال رمز التحقق: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<ServiceResponse<bool>> VerifySignatureOTPAsync(int signatureId, string otp)
        {
            try
            {
                var signature = await _context.Signatures
                    .FirstOrDefaultAsync(s => s.Id == signatureId);

                if (signature == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Signature not found",
                        Data = false
                    };
                }

                // التحقق من وجود OTP
                if (string.IsNullOrEmpty(signature.OTPCode))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "No OTP requested for this signature",
                        Data = false
                    };
                }

                // التحقق من انتهاء الصلاحية
                if (!signature.OTPExpiry.HasValue || signature.OTPExpiry.Value < DateTime.UtcNow)
                {
                    // تنظيف OTP المنتهي
                    signature.OTPCode = null;
                    signature.OTPExpiry = null;
                    await _context.SaveChangesAsync();

                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "OTP has expired",
                        Data = false
                    };
                }

                // التحقق من صحة OTP
                if (signature.OTPCode != otp)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid OTP",
                        Data = false
                    };
                }

                // OTP صحيح - تنظيف الحقول
                signature.OTPCode = null;
                signature.OTPExpiry = null;
                await _context.SaveChangesAsync();

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = "OTP verified successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = false
                };
            }
        }

        public async Task<ServiceResponse<Signature>> CompleteSignatureAsync(int signatureId)
        {
            try
            {
                var signature = await _context.Signatures
                    .Include(s => s.PurchaseOrders)
                    .FirstOrDefaultAsync(s => s.Id == signatureId);

                if (signature == null)
                {
                    return new ServiceResponse<Signature>
                    {
                        Success = false,
                        Message = "Signature not found",
                        Data = null
                    };
                }

                // تحديث تاريخ التوقيع
                signature.SignedDate = DateTime.UtcNow;

                // هنا يمكنك إضافة أي منطق إضافي للتوقيع
                // مثل تحديث حالة PurchaseOrders المرتبطة

                await _context.SaveChangesAsync();

                return new ServiceResponse<Signature>
                {
                    Success = true,
                    Message = "Signature completed successfully",
                    Data = signature
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Signature>
                {
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
