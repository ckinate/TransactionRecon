using Reconciliation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.SpecificErrors
{
    public static class PermissionErrors
    {
        public static Error NotFound(Guid name) => Error.NotFound(
      "Permissions.NotFound",
      $"The permission with the Name = '{name}' was not found");

        public static Error Exist(string userId, string permissionName) => Error.Conflict(
      "Permssions.Exist",
      $"The Permission with the Name = '{permissionName}' already exist for userId = {userId}");
    }
}
