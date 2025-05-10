using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class UserPermission
    {
        public required string UserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsGranted { get; set; } // True = granted, False = denied (overrides role permissions)

        // Navigation properties
        public virtual required ApplicationUser User { get; set; }
        public virtual required Permission Permission { get; set; }
    }
}
