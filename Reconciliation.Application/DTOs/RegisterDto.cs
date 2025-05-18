using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.DTOs
{
    public record RegisterDto(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, string? Role);
   
}
