using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public class ApplicationUserDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public virtual ICollection<UserPermissionDto>? UserPermissions { get; set; }
        public virtual ICollection<RefreshTokenDto>? RefreshTokens { get; set; }
    }
}
