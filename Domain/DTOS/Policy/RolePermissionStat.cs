using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Policy
{
    public class RolePermissionStat
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public int PermissionsCount { get; set; }
        public List<string> Permissions { get; set; }
    }
}
