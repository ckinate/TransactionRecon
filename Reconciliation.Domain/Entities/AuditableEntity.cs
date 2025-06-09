using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class AuditableEntity : EntityBase
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? LastModifyBy { get; set; }
        public DateTime? LastModifyDate { get; set; }
    }
}
