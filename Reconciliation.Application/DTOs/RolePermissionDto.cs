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
        public int Id { get; set; }
        public string PermissionName { get; set; }
    }
}
