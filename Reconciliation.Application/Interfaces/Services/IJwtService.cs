using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Interfaces.Services
{
    public interface IJwtService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);
    }
}
