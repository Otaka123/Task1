using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.OTP
{
    public class SignOTPRequest
    {
        public int SignatureId { get; set; }
        public string AdminId { get; set; }
        public string MobileNumber { get; set; }
    }

}
