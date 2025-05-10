using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class ApplicationUserMapper
    {
        public static ApplicationUserDto ToDto(this ApplicationUser entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null)
                return null;

            var dto = new ApplicationUserDto
            {
                Id = entity.Id,
                UserName = entity.UserName,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                UserPermissions = new List<UserPermissionDto>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren) && entity.UserPermissions != null)
            {
                // When mapping children, don't map their parents (to avoid circular references)
                dto.UserPermissions = entity.UserPermissions
                    .Select(up => up.ToDto(MappingDepth.None))
                    .ToList();
              

                // Manually set the User reference to avoid circular reference
                foreach (var userPermission in dto.UserPermissions)
                {
                    userPermission.User = dto;
                }
              
            }
            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren) && entity.RefreshTokens != null)
            {
                dto.RefreshTokens = entity.RefreshTokens.Select(up => up.ToDto(MappingDepth.None)).ToList();

                foreach (var refreshToken in dto.RefreshTokens)
                {
                    refreshToken.User = dto;
                }

            }

                return dto;
        }

        public static ApplicationUser ToEntity(this ApplicationUserDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new ApplicationUser
            {
                Id = dto.Id,
                UserName = dto.UserName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserPermissions = new List<UserPermission>(),
                RefreshTokens = new List<RefreshToken>()
            };

            // Only map child collections if requested
            if (depth.HasFlag(MappingDepth.IncludeChildren) && dto.UserPermissions != null)
            {
                // When mapping children, don't map their parents (to avoid circular references)
                entity.UserPermissions = dto.UserPermissions
                    .Select(up => up.ToEntity(MappingDepth.None))
                    .ToList();
               

                // Manually set the User reference to avoid circular reference
                foreach (var userPermission in entity.UserPermissions)
                {
                    userPermission.User = entity;
                }
              

            }
            if (depth.HasFlag(MappingDepth.IncludeChildren) && dto.RefreshTokens != null)
            {
                entity.RefreshTokens = dto.RefreshTokens.Select(up => up.ToEntity(MappingDepth.None)).ToList();
                foreach (var refreshToken in entity.RefreshTokens)
                {
                    refreshToken.User = entity;
                }

            }

                return entity;
        }

        public static IEnumerable<ApplicationUserDto> ToDtos(this IEnumerable<ApplicationUser> entities, MappingDepth depth = MappingDepth.Default)
        {
            return entities?.Select(e => e.ToDto(depth)).ToList() ?? new List<ApplicationUserDto>();
        }

        public static IEnumerable<ApplicationUser> ToEntities(this IEnumerable<ApplicationUserDto> dtos, MappingDepth depth = MappingDepth.Default)
        {
            return dtos?.Select(d => d.ToEntity(depth)).ToList() ?? new List<ApplicationUser>();
        }

        public static void UpdateEntityFromDto(this ApplicationUser entity, ApplicationUserDto dto)
        {
            if (entity == null || dto == null)
                return;

            entity.UserName = dto.UserName;
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            // Note: Collections usually require more complex handling in updates
        }
    }
}
