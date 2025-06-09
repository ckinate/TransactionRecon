using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Entities
{
    public abstract class EntityBase
    {
        public int Id { get; set; }
       // public Guid Id { get; set; }

        //public static Guid GenerateIdentity()
        //{
        //    return Guid.NewGuid();
        //   // return Guid.CreateVersion7();
        //}
    }
}
