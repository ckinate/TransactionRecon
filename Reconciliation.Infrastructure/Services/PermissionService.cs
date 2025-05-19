using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reconciliation.Application.DTOs;
using Reconciliation.Application.Interfaces.Repository;
using Reconciliation.Application.Interfaces.Services;
using Reconciliation.Domain.Common;
using Reconciliation.Domain.Entities;
using Reconciliation.Domain.SpecificErrors;
using Reconciliation.Infrastructure.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Reconciliation.Infrastructure.Authorization.Permissions;

namespace Reconciliation.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IGenericRepository<RolePermission> _rolePermissionRepository;
        private readonly IGenericRepository<UserPermission> _userPermissionRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService( UserManager<ApplicationUser> userManager,
           IGenericRepository<RolePermission> rolePermissionRepository, IGenericRepository<UserPermission> userPermissionRepository)
        {
            _userManager = userManager;
            _rolePermissionRepository = rolePermissionRepository;
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<ResultValue<bool>> AddPermissionToRoleAsync(string roleId, string permission)
        {
            // Check if permission already exists for role
            var exists = await _rolePermissionRepository.GetAll(false)
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionName == permission);

            if (exists)
            {
                return ResultValue<bool>.Fail($"The permission with Name = {permission} already exist for roleId {roleId}");
            }

           _rolePermissionRepository .Add(new RolePermission
            {
                RoleId = roleId,
                PermissionName = permission
            });

            await _rolePermissionRepository.SaveChangesAsync();
            return ResultValue<bool>.Ok(true,"Permission added successfully");
        }

        public async Task<ResultValue <bool>> AddPermissionToUserAsync(string userId, string permission)
        {
            // Check if permission already exists
            var exists = await _userPermissionRepository.GetAll(false)
                .AnyAsync(up => up.UserId == userId && up.PermissionName == permission);

            if (exists)
            {
                return ResultValue<bool>.Fail($"The Permission with the Name = '{permission}' already exist for userId = {userId}");
             //  throw new AlreadyExitException(PermissionErrors.Exist(userId,permission));
                
            }
            _userPermissionRepository.Add(new UserPermission
            {
                UserId = userId,
                PermissionName = permission
            });
            await _userPermissionRepository.SaveChangesAsync();

            return ResultValue<bool>.Ok(true,"Created Successfully");
        }

        public async Task<List<string>> GetAllDefinedPermissionsAsync()
        {
            // Collect all permissions from the static Permissions class using reflection
            var permissionProperties = typeof(Permissions)
                .GetNestedTypes()
                .SelectMany(type => type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy))
                .Where(field => field.IsLiteral && !field.IsInitOnly)
                .Select(field => field.GetValue(null).ToString())
                .ToList();

            return await Task.FromResult(permissionProperties);
        }

        public async Task<List<PermissionNode>> GetPermissionsTreeAsync()
        {
            // Get flat list of permissions
            var allPermissions = await GetAllDefinedPermissionsAsync();
            // Convert to tree structure
            var permissionTree = new List<PermissionNode>();
            var permissionGroups = new Dictionary<string, PermissionNode>();
            foreach (var permission in allPermissions)
            {
                var parts = permission.Split('.');
                if (parts.Length != 2)
                    continue;

                var groupName = parts[0];
                var actionName = parts[1];

                // Create group if it doesn't exist
                if (!permissionGroups.ContainsKey(groupName))
                {
                    var groupNode = new PermissionNode
                    {
                        Name = groupName,
                        Key = groupName,
                        IsGroup = true
                    };
                    permissionGroups[groupName] = groupNode;
                    permissionTree.Add(groupNode);
                }

                // Add permission to group
                permissionGroups[groupName].Children.Add(new PermissionNode
                {
                    Name = actionName,
                    Key = permission,
                    IsGroup = false
                });
            }

            return permissionTree;
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId)
        {
            return await _rolePermissionRepository.GetAll(false)
                 .Where(rp => rp.RoleId == roleId).Select(rp => rp.PermissionName).ToListAsync();
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            // Get direct user permissions
            var userPermissions = await _userPermissionRepository.GetAll(false)
                                        .Where(up => up.UserId == userId)
                                        .Select(up => up.PermissionName)
                                        .ToListAsync();
            // Get user's roles
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(UserErrors.NotFound(userId));
            }
            var userRoles = await _userManager.GetRolesAsync(user);

            // Get all permissions from user's roles
            var rolePermissions = await _rolePermissionRepository.GetAll(false)
                .Where(rp => userRoles.Contains(rp.RoleId)).Select(rp => rp.PermissionName).Distinct().ToListAsync();

            // Combine role permissions with granted user permission
            var result = userPermissions.Union(rolePermissions).ToList();

            return result;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            // First check if the user has this permission explicitly denied
            var userPermission = await _userPermissionRepository.GetAll(false).FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionName == permissionName);
            if (userPermission == null)
            {
                return false; // Explicitly denied
            }
            if (userPermission != null)
            {
                return true; // Explicitly granted
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(UserErrors.NotFound(userId));
            }
            var userRoleIds = await _userManager.GetRolesAsync(user);

            return await _rolePermissionRepository.GetAll(false).AnyAsync(rp => userRoleIds.Contains(rp.RoleId) && rp.PermissionName == permissionName);
        }

        public async Task<ResultValue<bool>> RemovePermissionFromRoleAsync(string roleId, string permission)
        {
            var rolePermission = await _rolePermissionRepository.GetAll(false)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionName == permission);

            if (rolePermission == null)
            {
                return ResultValue<bool>.Fail($"The Permission with name {permission} does not exist for roleId {roleId}");;
            }

           _rolePermissionRepository.Delete (rolePermission);
            await _rolePermissionRepository.SaveChangesAsync();
            return ResultValue<bool>.Ok(true,"Permission removed successfully");
        }

        public async Task<ResultValue<bool>> RemovePermissionFromUserAsync(string userId, string permission)
        {
            var userPermission = await _userPermissionRepository.GetAll(false)
           .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionName == permission);

            if (userPermission == null)
            {
                return ResultValue<bool>.Fail($"The Permission with name {permission} does not exist for userId {userId}");
            }

            _userPermissionRepository.Delete(userPermission);
            await _userPermissionRepository.SaveChangesAsync();
            return ResultValue<bool>.Ok(true,"Permission remove successfully");
        }

       
    }
}
