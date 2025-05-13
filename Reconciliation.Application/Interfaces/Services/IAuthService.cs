using Reconciliation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginDto model);
        Task<AuthResult> RegisterAsync(RegisterDto model);
        Task<AuthResult> RefreshTokenAsync(string token, string refreshToken);
        Task RevokeTokenAsync(string refreshToken);
    }
}
