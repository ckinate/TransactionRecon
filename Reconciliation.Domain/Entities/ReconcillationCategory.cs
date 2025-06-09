using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public class ReconcillationCategory
    {
        public string CategoryName { get; set; }

        public ReconcillationCategory(string _categoryName)
        {
            CategoryName = _categoryName;

        }
        public ReconcillationCategory()
        {

        }
    }
}
