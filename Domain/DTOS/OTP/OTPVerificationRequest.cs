using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{
    public class OTPVerificationRequest
    {
        public string MobileNumber { get; set; }
        public string OTP { get; set; }
        public int SignatureId { get; set; } // إضافة لربط التحقق بالتوقيع

    }

}
