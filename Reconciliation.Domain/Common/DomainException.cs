using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Common
{
    public class DomainException : Exception
    {
        public Error Error { get; }

        public DomainException(Error error) : base(error.Description)
        {
            Error = error;
        }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(Error error) : base(error) { }
    }

    public class UnauthorizedException : DomainException
    {
        public UnauthorizedException(Error error) : base(error) { }
    }
    public class AlreadyExitException : DomainException
    {
        public AlreadyExitException(Error error) : base(error) { }
    }

}
