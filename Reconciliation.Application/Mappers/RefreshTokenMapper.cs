using Reconciliation.Application.DTOs;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public static class RefreshTokenMapper
    {
        public static RefreshTokenDto ToDto(this RefreshToken entity, MappingDepth depth = MappingDepth.Default)
        {
            if (entity == null) return null;
            var dto = new RefreshTokenDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                IsRevoked = entity.IsRevoked,
                ExpiryDate = entity.ExpiryDate,
                Token = entity.Token,
                User = null
                // User = entity.User 
            };
            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                dto.User = entity.User?.ToDto(MappingDepth.None);
                
            }




            return dto;


        }

        public static RefreshToken ToEntity(this RefreshTokenDto dto, MappingDepth depth = MappingDepth.Default)
        {
            if (dto == null)
                return null;

            var entity = new RefreshToken
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Token = dto.Token,
                ExpiryDate = dto.ExpiryDate,
                IsRevoked = dto.IsRevoked,
                User = null,

            };

            // Only map parent objects if requested
            if (depth.HasFlag(MappingDepth.IncludeParents))
            {
                // When mapping parents, don't map their children (to avoid circular references)
                entity.User = dto.User?.ToEntity(MappingDepth.None);
                
            }

            return entity;

        }
    }
}
