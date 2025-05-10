using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Mappers
{
    public enum MappingDepth
    {
        None = 0,
        IncludeChildren = 1,      // Include direct children (one level down)
        IncludeParents = 2,       // Include direct parents (one level up)
        Default = IncludeChildren // Default behavior
    }


    // Basic mapping (includes children but not parents)
   // var roleDto = role.ToDto();

    // Deep mapping (includes both children and parents - use with caution)
  //  var roleDto = role.ToDto(MappingDepth.IncludeChildren | MappingDepth.IncludeParents);

    // Shallow mapping (just the entity itself, no related objects)
  //  var roleDto = role.ToDto(MappingDepth.None);

    // Map a collection with specific depth
   // var userDtos = users.ToDtos(MappingDepth.IncludeChildren);
}
