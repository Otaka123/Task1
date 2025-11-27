using Application.Interfaces;
using Domain.DTOS.OTP;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Application.Services.SMS
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private static readonly ConcurrentDictionary<string, OTPData> _otpStorage = new();
        private readonly Random _random = new();

        public SmsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<ServiceResponse<string>> SendSMSAsync(string mobileNumber, string message)
        {
            try
            {
                string url = $"http://smsapi.esmart-vision.com/api/mim/SendSMS" +
                             $"?userid=SMV2300001" +
                             $"&pwd=DD8D$803_C91" +
                             $"&mobile={Uri.EscapeDataString(mobileNumber)}" +
                             $"&sender=FFRD" +
                             $"&msg={Uri.EscapeDataString(message)}" +
                             $"&msgtype=20";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return new ServiceResponse<string>
                {
                    Success = true,
                    Message = "SMS sent successfully",
                    Data = responseBody
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = $"Failed to send SMS: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<ServiceResponse<string>> SendOTPAsync(string phoneNumber, string otp)
        {
            var message = $@"كود التحقق لتوقيع أمر الشراء

عزيزي المستخدم،

كود التحقق الخاص بك هو: {otp}

هذا الكود صالح لمدة 10 دقائق فقط.

إذا لم تطلب هذا الكود، يرجى تجاهل هذه الرسالة.

هذه رسالة آلية، يرجى عدم الرد عليها.
- نادي الفجيرة العلمي";

            return await SendSMSAsync(phoneNumber, message);
        }
        public async Task<ServiceResponse<string>> SendOTPAsync(string mobileNumber)
        {
            // Generate 6-digit OTP
            string otp = _random.Next(100000, 999999).ToString();

            // Store OTP with expiration (5 minutes)
            _otpStorage[mobileNumber] = new OTPData
            {
                Code = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(5),
                Attempts = 0
            };

            string message = $"Your verification code is: {otp}. This code will expire in 5 minutes.";

            var smsResult = await SendSMSAsync(mobileNumber, message);

            if (smsResult.Success)
            {
                return new ServiceResponse<string>
                {
                    Success = true,
                    Message = "OTP sent successfully",
                    Data = otp // In production, don't return the actual OTP
                };
            }

            return new ServiceResponse<string>
            {
                Success = false,
                Message = smsResult.Message,
                Data = null
            };
        }

        public Task<ServiceResponse<bool>> VerifyOTPAsync(string mobileNumber, string otp)
        {
            if (!_otpStorage.TryGetValue(mobileNumber, out var otpData))
            {
                return Task.FromResult(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "OTP not found or expired",
                    Data = false
                });
            }

            // Check if OTP is expired
            if (DateTime.UtcNow > otpData.ExpirationTime)
            {
                _otpStorage.TryRemove(mobileNumber, out _);
                return Task.FromResult(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "OTP has expired",
                    Data = false
                });
            }

            // Check attempts
            if (otpData.Attempts >= 3)
            {
                _otpStorage.TryRemove(mobileNumber, out _);
                return Task.FromResult(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Too many failed attempts",
                    Data = false
                });
            }

            // Verify OTP
            if (otpData.Code == otp)
            {
                _otpStorage.TryRemove(mobileNumber, out _);
                return Task.FromResult(new ServiceResponse<bool>
                {
                    Success = true,
                    Message = "OTP verified successfully",
                    Data = true
                });
            }
            else
            {
                otpData.Attempts++;
                return Task.FromResult(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Invalid OTP",
                    Data = false
                });
            }
        }
    }
}
