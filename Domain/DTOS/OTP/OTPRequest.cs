using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{
    public class OTPRequest
    {
        public string MobileNumber { get; set; }
        public string Message { get; set; }
        public int SignatureId { get; set; } // إضافة لربط OTP بالتوقيع

    }

}
