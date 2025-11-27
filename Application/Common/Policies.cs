using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class Policies
    {
        public static IEnumerable<string> GetAllPolicies()
        {
            return typeof(Policies)
                .GetNestedTypes()
                //.Where(t => t.Name != nameof(Roles)) // 👈 استثناء كلاس Roles
                .SelectMany(t => t.GetFields()
                    .Select(f => f.GetValue(null)?.ToString()))
                .Where(v => v != null)!
                .Cast<string>();
        }
    }
}
