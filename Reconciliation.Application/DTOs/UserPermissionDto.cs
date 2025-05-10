using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public class UserPermissionDto
    {
        public required string UserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsGranted { get; set; } // True = granted, False = denied (overrides role permissions)

        // Navigation properties
        public virtual required ApplicationUserDto User { get; set; }
        public virtual required PermissionDto Permission { get; set; }
    }
}
