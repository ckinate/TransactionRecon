using Reconciliation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<bool> HasPermissionAsync(string userId, string permissionName);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId);
        Task<bool> GrantPermissionToUserAsync(string userId, string permissionName);
        Task<bool> RevokePermissionFromUserAsync(string userId, string permissionName);
        Task<bool> GrantPermissionToRoleAsync(string roleId, string permissionName);
        Task<bool> RevokePermissionFromRoleAsync(string roleId, string permissionName);
    }
}
