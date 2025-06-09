using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class Bank: AuditableEntity
    {
        public string Name { get; set; }
        public string BankCode { get; set; }
    }
}
