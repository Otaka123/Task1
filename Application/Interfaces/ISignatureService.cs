using Domain.DTOS.OTP;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISignatureService
    {
        Task<ServiceResponse<string>> SendSignatureOTPAsync(int signatureId, string adminId, string mobileNumber);
        Task<ServiceResponse<bool>> VerifySignatureOTPAsync(int signatureId, string otp);
        Task<ServiceResponse<Signature>> CompleteSignatureAsync(int signatureId);
    }
}
