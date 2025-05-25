using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<List<string>> GetAllDefinedPermissionsAsync();
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<ResultValue< bool>> AddPermissionToUserAsync(string userId, string permission);
        Task<ResultValue<bool>> RemovePermissionFromUserAsync(string userId, string permission);
        Task<ResultValue<bool>> AddPermissionToRoleAsync(string roleId, string permission);
        Task<ResultValue<bool>> RemovePermissionFromRoleAsync(string roleId, string permission);
        Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId);
        Task<bool> HasPermissionAsync(string userId, string permissionName);
        Task<List<PermissionNode>> GetPermissionsTreeAsync();
    }
}
