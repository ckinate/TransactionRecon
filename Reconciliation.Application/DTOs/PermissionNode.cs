using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public class PermissionNode
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public bool IsGroup { get; set; }
        public List<PermissionNode> Children { get; set; } = new List<PermissionNode>();
    }
}
