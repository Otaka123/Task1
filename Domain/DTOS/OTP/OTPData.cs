using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{
    public class OTPData
    {
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int Attempts { get; set; }
    }
}
