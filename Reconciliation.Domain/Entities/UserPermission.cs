using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
    
        public string PermissionName { get; set; }
      

        // Navigation properties
        public virtual ApplicationUser User { get; set; }

    }
}
