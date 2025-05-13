using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class RolePermission
    {
        public int Id { get; set; }
        public required string RoleId { get; set; }
        public string PermissionName { get; set; }

        // Navigation properties
        public virtual  ApplicationRole Role { get; set; }
       
    }
}
