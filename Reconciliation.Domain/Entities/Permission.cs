using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public  string Description { get; set; }

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; }
    }
}
