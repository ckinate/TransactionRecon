using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class ApplicationRoleMapper
    {
        public static ApplicationRoleDto ToDto(this ApplicationRole entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null)
                return null;

            var dto = new ApplicationRoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                RolePermissions = new List<RolePermissionDto>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren) && entity.RolePermissions != null)
            {
                // When mapping children, don't map their parents (to avoid circular references)
                // This prevents Role -> RolePermission -> Role infinite recursion
                dto.RolePermissions = entity.RolePermissions
                    .Select(rp => rp.ToDto(MappingDepth.None))
                    .ToList();

                // Manually set the Role reference to avoid circular reference
                foreach (var rolePermission in dto.RolePermissions)
                {
                    rolePermission.Role = dto;
                }
            }

            return dto;
        }

        public static ApplicationRole ToEntity(this ApplicationRoleDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new ApplicationRole
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                RolePermissions = new List<RolePermission>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren) && dto.RolePermissions != null)
            {
                // When mapping children, don't map their parents (to avoid circular references)
                entity.RolePermissions = dto.RolePermissions
                    .Select(rp => rp.ToEntity(MappingDepth.None))
                    .ToList();

                // Manually set the Role reference to avoid circular reference
                foreach (var rolePermission in entity.RolePermissions)
                {
                    rolePermission.Role = entity;
                }
            }

            return entity;
        }
        public static IEnumerable<ApplicationRoleDto> ToDtos(this IEnumerable<ApplicationRole> entities, MappingDepth depth = MappingDepth.Default)
        {
            return entities?.Select(e => e.ToDto(depth)).ToList() ?? new List<ApplicationRoleDto>();
        }

        public static IEnumerable<ApplicationRole> ToEntities(this IEnumerable<ApplicationRoleDto> dtos, MappingDepth depth = MappingDepth.Default)
        {
            return dtos?.Select(d => d.ToEntity(depth)).ToList() ?? new List<ApplicationRole>();
        }

        public static void UpdateEntityFromDto(this ApplicationRole entity, ApplicationRoleDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            // Note: Collections usually require more complex handling in updates
        }
    }
}
