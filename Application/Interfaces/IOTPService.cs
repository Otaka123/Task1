using Domain.DTOS.OTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOTPService
    {
        //Task<OTPGenerationResult> GenerateAndSendOTPAsync(int userId, string email, int purchaseOrderId);
        Task<OTPGenerationResult> GenerateAndSendOTPAsync(string userId, string phoneNumber, int purchaseOrderId);

        Task<OTPVerificationResult> VerifyOTPAsync(int purchaseOrderId, string otp, string userid);
        Task<bool> ConsumeOTPAsync(int purchaseOrderId, string otp);
        //Task<bool> IsOTPValidAsync(int purchaseOrderId, string otp);
        Task<string> GetStoredOTPAsync(int purchaseOrderId);
        Task<bool> IsOTPValidAsync(int purchaseOrderId, string otp, string userid);
    }

}
