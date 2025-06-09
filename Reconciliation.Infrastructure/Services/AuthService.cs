using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Reconciliation.Application.DTOs;
using Reconciliation.Application.Interfaces.Repository;
using Reconciliation.Application.Interfaces.Services;
using Reconciliation.Domain.Entities;
using Reconciliation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Reconciliation.Infrastructure.Authorization.Permissions;

namespace Reconciliation.Infrastructure.Services
{
    public class AuthService: IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IGenericRepository<UserPermission> _userPermissionRepository;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<RolePermission> _rolePermissionRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IGenericRepository<UserPermission> userPermissionRepository,
    IGenericRepository<RefreshToken> refreshTokenRepository, IConfiguration configuration,
    IGenericRepository<RolePermission> rolePermissionRepository, IEmailService emailService,
    ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userPermissionRepository = userPermissionRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _rolePermissionRepository = rolePermissionRepository;
            _emailService = emailService;
            _logger = logger;
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
                    Message = "Your account is not active. Please verify your email address if you have not."
                };
            }

            // Check if email is confirmed
            if (!user.EmailConfirmed)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Please verify your email address before logging in."
                };
            }

            // Update last login timestamp
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Get user permissions (both direct and from roles)
            var permissions = await GetUserPermissionsAsync(user.Id);

            // Generate JWT token
            var (token, expiration) = GenerateJwtToken(user, userRoles, permissions);
            // Refresh token management
            string refreshToken;

            // Initialize RefreshTokens collection if null
            user.RefreshTokens ??= new List<RefreshToken>();
            var allExistingValidToken = await _refreshTokenRepository.GetAll(false).Where(rt => rt.UserId == user.Id ).ToListAsync();

            // Check if user already has a valid refresh token that we can reuse
            var existingValidToken = allExistingValidToken.OrderByDescending(rt => rt.ExpiryDate).FirstOrDefault(rt => rt.ExpiryDate > DateTime.UtcNow.AddDays(1));// Still has at least 1 day validity
          

            if (existingValidToken != null)
            {
                // Reuse existing token
                refreshToken = existingValidToken.Token;
            }
            else
            {
                // Generate a new refresh token
                refreshToken = GenerateRefreshToken();

                // Ensure we don't keep too many tokens per user (limit to 4 most recent)
                const int maxTokensPerUser = 4;
                if (allExistingValidToken.Count >= maxTokensPerUser)
                {
                    // Remove oldest tokens, keeping only the most recent ones
                    var tokensToRemove = allExistingValidToken
                        .OrderByDescending(rt => rt.ExpiryDate)
                        .Skip(maxTokensPerUser - 1) // Skip the N-1 most recent tokens (I am adding one)
                        .ToList();

                    foreach (var oldToken in tokensToRemove)
                    {
                      
                        _refreshTokenRepository.Delete(oldToken);
                    }
                }

                // Add the new token
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

                await _refreshTokenRepository.SaveChangesAsync();
            }

            return new AuthResult
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpireAt = expiration,
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = userRoles.ToList(),
                Permissions = permissions
            };

        }

        public async Task<AuthResult> RegisterAsync(RegisterDto model)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User with this email already exists"
                };
            }
            // Create new user
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = false, // User starts as INACTIVE
                EmailConfirmed = false // ASP.NET Identity built-in property
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign default role if specified
            if (!string.IsNullOrEmpty(model.Role))
            {
                if (await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "User");

            }

            // Generate email confirmation token
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // Create verification link
            var encodedToken = WebUtility.UrlEncode(emailConfirmationToken);
           // var verificationLink = $"{_configuration["AppSettings:BaseUrl"]}/auth/verify-email?userId={user.Id}&token={encodedToken}";
            var verificationLink = $"{_configuration["AppSettings:FrontendUrl"]}/verify-email?userId={user.Id}&token={encodedToken}";

            // Send verification email
            try
            {
                await _emailService.SendEmailVerificationAsync(user.Email, verificationLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send verification email to {user.Email}");
                // Optionally delete the user if email sending fails
                await _userManager.DeleteAsync(user);
                return new AuthResult
                {
                    Success = false,
                    Message = "Failed to send verification email. Please try again."
                };
            }
            // DO NOT return tokens yet - user needs to verify email first
            return new AuthResult
            {
                Success = true,
                Message = "Registration successful! Please check your email to verify your account before logging in.",
                UserId = user.Id,
                Email = user.Email
                // No tokens provided until email is verified
            };

            // Generate tokens
            //var roles = await _userManager.GetRolesAsync(user);
            //var permissions = await GetUserPermissionsAsync(user.Id);
            //var (token, expiration) = GenerateJwtToken(user, roles, permissions);
            //var refreshToken = GenerateRefreshToken();

            // Save refresh token
        //    user.RefreshTokens = new List<RefreshToken>
        //{
        //    new RefreshToken
        //    {
        //        Token = refreshToken,
        //        ExpiryDate = DateTime.UtcNow.AddDays(7),
        //        UserId = user.Id
        //    }
       // };

            //await _refreshTokenRepository.SaveChangesAsync();
            //return new AuthResult
            //{
            //    Success = true,
            //    Token = token,
            //    RefreshToken = refreshToken,
            //    ExpireAt = expiration,
            //    UserId = user.Id,
            //    Email = user.Email,
            //    Roles = roles.ToList(),
            //    Permissions = permissions
            //};
        }



        public async Task<AuthResult> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }

            var existingRefreshToken = _refreshTokenRepository.GetAll(false)
                .SingleOrDefault(rt => rt.Token == refreshToken && rt.UserId == userId && !rt.IsRevoked);

            if (existingRefreshToken == null || existingRefreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                };
            }

            // Get user roles and permissions
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissionsAsync(user.Id);

            // Generate new tokens
            var (newToken, expiration) = GenerateJwtToken(user, roles, permissions);
            var newRefreshToken = GenerateRefreshToken();

            // Revoke old refresh token and add new one
            existingRefreshToken.IsRevoked = true;

            _refreshTokenRepository.Add(new RefreshToken
            {
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                UserId = user.Id
            });

            await _refreshTokenRepository.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpireAt = expiration,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToList(),
                Permissions = permissions
            };
        }
        public async Task RevokeTokenAsync(string refreshToken)
        {
            var token = _refreshTokenRepository.GetAll(false).SingleOrDefault(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (token == null)
            {
                throw new Exception("Invalid token");
            }

            // Revoke token
            token.IsRevoked = true;
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task<AuthResult> VerifyEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid verification link"
                };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Invalid verification link"
                };
              
            }

            if (user.EmailConfirmed)
            {
                return new AuthResult
                {
                    Success = true,
                    Message = "Email already verified. You can now login."
                };
              
            }
            var decodedToken = WebUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                // Activate the user account
                user.IsActive = true;
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // Redirect to success page or return success response
                return new AuthResult
                {
                    Success = true,
                    Message = "Email verified successfully! Your account is now active. You can login now"
                };
               
            }
            return new AuthResult
            {
                Success = false,
                Message = "Email verification failed. The link may be expired or invalid."
            };
        }

        public async Task<AuthResult> ResendVerificationEmail(ResendVerificationDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal if user exists or not for security
                return new AuthResult
                {
                    Success = true,
                    Message = "If the email exists in our system, a verification email has been sent."
                };
            
            }

            if (user.EmailConfirmed)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email is already verified."
                };
               
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(emailConfirmationToken);
            var verificationLink = $"{_configuration["AppSettings:FrontendUrl"]}/verify-email?userId={user.Id}&token={encodedToken}";

            try
            {
                await _emailService.SendEmailVerificationAsync(user.Email, verificationLink);
                return new AuthResult
                {
                    Success = true,
                    Message = "Verification email sent successfully."
                };
              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to resend verification email to {user.Email}");
                return new AuthResult
                {
                    Success = false,
                    Message = "Failed to send verification email."
                };
              
            }
        }







        private async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            // Get direct user permissions
            var userPermissions = await _userPermissionRepository.GetAll(false)
                .Where(up => up.UserId == userId)
                .Select(up => up.PermissionName)
                .ToListAsync();

            // Get user roles
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);

            // Get permissions from roles
            var rolePermissions = await _rolePermissionRepository.GetAll(false)
                .Where(rp => roles.Contains(rp.Role.Name))
                .Select(rp => rp.PermissionName)
                .ToListAsync();
            

            // Combine and remove duplicates
            return userPermissions.Union(rolePermissions).Distinct().ToList();
        }

        private (string token, DateTime expiration) GenerateJwtToken(
       ApplicationUser user,
       IEnumerable<string> roles,
       IEnumerable<string> permissions)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Add roles as claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permissions as claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationHours"]));
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"])),
                ValidateLifetime = false // Allow expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }

}
