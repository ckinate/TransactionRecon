using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Reconciliation.Application.Interfaces.Repository;
using Reconciliation.Application.Interfaces.Services;
using Reconciliation.Domain.Entities;
using Reconciliation.Infrastructure.Authorization;
using Reconciliation.Infrastructure.Data;
using Reconciliation.Infrastructure.Repositories;
using Reconciliation.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
        {
            services.AddDatabase(configuration)
                    .AddServices()
                    .AddIdentityServices(configuration)
                    .AddAuthenticationInternal(configuration)
                    .AddAuthorizationInternal();

            return services;
        }



        private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
               options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
           // services.AddSingleton<ILoggerService, LoggerService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPermissionService, PermissionService>();


            return services;
        }
        private static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityCore<ApplicationUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
                // opt.Password.RequireDigit = true;
                // opt.Password.RequiredLength = 8;
                //  opt.Password.RequireNonAlphanumeric = true;
                //  opt.Password.RequireUppercase = true;
                // opt.Password.RequireLowercase = true;
                // Lockout settings
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                // User settings
                opt.User.RequireUniqueEmail = true;
            }).AddRoles<ApplicationRole>()
            .AddRoleManager<RoleManager<ApplicationRole>>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();


            return services;
        }
        private static IServiceCollection AddAuthenticationInternal(
       this IServiceCollection services,
       IConfiguration _configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
        {
           options.TokenValidationParameters = new TokenValidationParameters
         {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = _configuration["JwtSettings:Issuer"],
        ValidAudience = _configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]))
        };
         });


            return services;
        }

        private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
        {
            services.AddAuthorization();
            // Configure Authorization with Permissions
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();


            return services;
        }
    }
}
