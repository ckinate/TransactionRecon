using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Reconciliation.Application.DTOs;
using Reconciliation.Application.Interfaces.Repository;
using Reconciliation.Domain.Entities;
using Reconciliation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IGenericRepository<UserPermission> _userPermissionRepository;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        public AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IGenericRepository<UserPermission> userPermissionRepository,
    IGenericRepository<RefreshToken> refreshTokenRepository, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userPermissionRepository = userPermissionRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }
            if (!user.IsActive)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User account is deactivated"
                };
            }
            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Get user permissions (both direct and from roles)
            var permissions = await GetUserPermissionsAsync(user.Id);

        }
    }

}
