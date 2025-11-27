using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class Admin : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime Createtime { get; set; }
        public Signature? Signature { get; set; }
        public string? JopTitle { get; set; }
        public string? phone {  get; set; }
        public DateTime? LastLoginDate { get; set; }

        public Admin()
        {
            Id=Guid.NewGuid().ToString();
            Createtime = DateTime.UtcNow;
        }
    }
}
