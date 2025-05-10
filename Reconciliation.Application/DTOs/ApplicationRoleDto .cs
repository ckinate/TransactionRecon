using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public class ApplicationRoleDto
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<RolePermissionDto> RolePermissions { get; set; }
    }
}
