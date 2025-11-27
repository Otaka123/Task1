using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOS.Policy
{
    public class RolePermissionsStatistics
    {
        public int TotalRoles { get; set; }
        public int RolesWithPermissions { get; set; }
        public int RolesWithoutPermissions { get; set; }
        public List<RolePermissionStat> RoleStats { get; set; }
    }
}
