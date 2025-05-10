using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public class RolePermissionDto
    {
        public required string RoleId { get; set; }
        public int PermissionId { get; set; }

        // Navigation properties
        public virtual required ApplicationRoleDto Role { get; set; }
        public virtual required PermissionDto Permission { get; set; }
    }
}
