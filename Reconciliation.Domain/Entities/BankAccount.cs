using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class BankAccount: AuditableEntity
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public int BankId { get; set; }

        // Navigation property
        public Bank BankName { get; set; }

    }
}
