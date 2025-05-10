using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class RolePermission
    {
        public required string RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public virtual required ApplicationRole Role { get; set; }
        public virtual required Permission Permission { get; set; }
    }
}
