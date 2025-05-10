using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class ApplicationRole : IdentityRole
    {
       
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
