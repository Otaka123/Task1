using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Policy
{
    public class ClaimDTO
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string Issuer { get; set; }
    }
}
