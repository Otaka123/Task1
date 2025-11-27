using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{

    public class OTPVerificationResult
    {
        public bool IsValid { get; set; }
        public Signature Signature {  get; set; }   
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
