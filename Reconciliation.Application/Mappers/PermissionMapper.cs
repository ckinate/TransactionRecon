using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class PermissionMapper
    {
        public static PermissionDto ToDto(this Permission entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null)
                return null;

            var dto = new PermissionDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                RolePermissions = new List<RolePermissionDto>(),
                UserPermissions = new List<UserPermissionDto>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren))
            {
                if (entity.RolePermissions != null)
                {
                    // When mapping children, don't map their parents (to avoid circular references)
                    dto.RolePermissions = entity.RolePermissions
                        .Select(rp => rp.ToDto(MappingDepth.None))
                        .ToList();

                    // Manually set the Permission reference to avoid circular reference
                    foreach (var rolePermission in dto.RolePermissions)
                    {
                        rolePermission.Permission = dto;
                    }
                }

                if (entity.UserPermissions != null)
                {
                    // When mapping children, don't map their parents (to avoid circular references)
                    dto.UserPermissions = entity.UserPermissions
                        .Select(up => up.ToDto(MappingDepth.None))
                        .ToList();

                    // Manually set the Permission reference to avoid circular reference
                    foreach (var userPermission in dto.UserPermissions)
                    {
                        userPermission.Permission = dto;
                    }
                }
            }

            return dto;
        }

        public static Permission ToEntity(this PermissionDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new Permission
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                RolePermissions = new List<RolePermission>(),
                UserPermissions = new List<UserPermission>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren))
            {
                if (dto.RolePermissions != null)
                {
                    // When mapping children, don't map their parents (to avoid circular references)
                    entity.RolePermissions = dto.RolePermissions
                        .Select(rp => rp.ToEntity(MappingDepth.None))
                        .ToList();

                    // Manually set the Permission reference to avoid circular reference
                    foreach (var rolePermission in entity.RolePermissions)
                    {
                        rolePermission.Permission = entity;
                    }
                }

                if (dto.UserPermissions != null)
                {
                    // When mapping children, don't map their parents (to avoid circular references)
                    entity.UserPermissions = dto.UserPermissions
                        .Select(up => up.ToEntity(MappingDepth.None))
                        .ToList();

                    // Manually set the Permission reference to avoid circular reference
                    foreach (var userPermission in entity.UserPermissions)
                    {
                        userPermission.Permission = entity;
                    }
                }
            }

            return entity;
        }
        public static IEnumerable<PermissionDto> ToDtos(this IEnumerable<Permission> entities, MappingDepth depth = MappingDepth.Default)
        {
            return entities?.Select(e => e.ToDto(depth)).ToList() ?? new List<PermissionDto>();
        }

        public static IEnumerable<Permission> ToEntities(this IEnumerable<PermissionDto> dtos, MappingDepth depth = MappingDepth.Default)
        {
            return dtos?.Select(d => d.ToEntity(depth)).ToList() ?? new List<Permission>();
        }

        public static void UpdateEntityFromDto(this Permission entity, PermissionDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            // Note: Collections usually require more complex handling in updates
        }
    }
}
