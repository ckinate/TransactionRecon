using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class UserPermissionMapper
    {
        public static UserPermissionDto ToDto(this UserPermission entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null)
                return null;

            var dto = new UserPermissionDto
            {
                UserId = entity.UserId,
                PermissionId = entity.PermissionId,
                IsGranted = entity.IsGranted,
                User = null,
                Permission = null
            };

            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                dto.User = entity.User?.ToDto(MappingDepth.None);
                dto.Permission = entity.Permission?.ToDto(MappingDepth.None);
            }

            return dto;
        }
        public static UserPermission ToEntity(this UserPermissionDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new UserPermission
            {
                UserId = dto.UserId,
                PermissionId = dto.PermissionId,
                IsGranted = dto.IsGranted,
                User = null,
                Permission = null
            };

            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                entity.User = dto.User?.ToEntity(MappingDepth.None);
                entity.Permission = dto.Permission?.ToEntity(MappingDepth.None);
            }

            return entity;
        }

        public static IEnumerable<UserPermissionDto> ToDtos(this IEnumerable<UserPermission> entities, MappingDepth depth = MappingDepth.Default)
        {
            return entities?.Select(e => e.ToDto(depth)).ToList() ?? new List<UserPermissionDto>();
        }

        public static IEnumerable<UserPermission> ToEntities(this IEnumerable<UserPermissionDto> dtos, MappingDepth depth = MappingDepth.Default)
        {
            return dtos?.Select(d => d.ToEntity(depth)).ToList() ?? new List<UserPermission>();
        }

        public static void UpdateEntityFromDto(this UserPermission entity, UserPermissionDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.UserId = dto.UserId;
            entity.PermissionId = dto.PermissionId;
            entity.IsGranted = dto.IsGranted;
            // Note: For navigation properties, you typically don't update them directly
        }
    }
}
