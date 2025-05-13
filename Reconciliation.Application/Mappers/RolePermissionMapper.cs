using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class RolePermissionMapper
    {
        public static RolePermissionDto ToDto(this RolePermission entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null)
                return null;

            var dto = new RolePermissionDto
            {
                RoleId = entity.RoleId,
             //   PermissionId = entity.PermissionId,
                Role = null,
                Permission = null
            };

            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                dto.Role = entity.Role?.ToDto(MappingDepth.None);
               // dto.Permission = entity.PermissionName?.ToDto(MappingDepth.None);
            }

            return dto;
        }
        public static RolePermission ToEntity(this RolePermissionDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new RolePermission
            {
                RoleId = dto.RoleId,
              //  PermissionId = dto.PermissionId,
                Role = null,
              //  Permission = null
            };

            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                entity.Role = dto.Role?.ToEntity(MappingDepth.None);
              //  entity.Permission = dto.Permission?.ToEntity(MappingDepth.None);
            }

            return entity;
        }

        public static IEnumerable<RolePermissionDto> ToDtos(this IEnumerable<RolePermission> entities, MappingDepth depth = MappingDepth.Default)
        {
            return entities?.Select(e => e.ToDto(depth)).ToList() ?? new List<RolePermissionDto>();
        }

        public static IEnumerable<RolePermission> ToEntities(this IEnumerable<RolePermissionDto> dtos, MappingDepth depth = MappingDepth.Default)
        {
            return dtos?.Select(d => d.ToEntity(depth)).ToList() ?? new List<RolePermission>();
        }

        public static void UpdateEntityFromDto(this RolePermission entity, RolePermissionDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.RoleId = dto.RoleId;
          //  entity.PermissionId = dto.PermissionId;
            // Note: For navigation properties, you typically don't update them directly
        }
    }
}
