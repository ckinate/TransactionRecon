using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        // Navigation property
        public virtual ICollection<UserPermission>? UserPermissions { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
