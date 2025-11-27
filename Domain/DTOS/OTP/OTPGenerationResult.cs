using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{
    public class OTPGenerationResult
    {
        public bool Success { get; set; }
        public string OTP { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
        public string Message {  get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
