using Domain.DTOS.OTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISmsService
    {
        Task<ServiceResponse<string>> SendSMSAsync(string mobileNumber, string message);
        Task<ServiceResponse<string>> SendOTPAsync(string mobileNumber);
        Task<ServiceResponse<bool>> VerifyOTPAsync(string mobileNumber, string otp);
    }
}
