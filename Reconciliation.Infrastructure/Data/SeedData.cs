using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reconciliation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
             ApplicationDbContext context,
           UserManager<ApplicationUser> userManager,
           RoleManager<ApplicationRole> roleManager
            )
        {
            // Create default roles if they don't exist
            string[] roleNames = { "Admin", "Manager", "User","Finance" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = roleName,
                        Description = $"Default {roleName} role"
                    });
                }
            }

            // Create admin user if it doesn't exist
            var adminUser = await userManager.FindByEmailAsync("admin@gmail.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Associate permissions with roles
            var adminRole = await roleManager.FindByNameAsync("Admin");
            var managerRole = await roleManager.FindByNameAsync("Manager");
            var userRole = await roleManager.FindByNameAsync("User");
            var financeRole = await roleManager.FindByNameAsync("Finance");

            // Associate permissions with Users
            var adminExistingUser = await userManager.FindByEmailAsync("admin@gmail.com");
            
            // Get all permissions
            var permissions = new List<Permission>()
            {
                 new Permission() { Name= "Users.View",Description = "Can View Users"},
                 new Permission() { Name= "Users.Create",Description = "Can Create Users"},
                 new Permission() { Name= "Users.Edit",Description = "Can Edit Users"},
                 new Permission() { Name= "Users.Delete",Description = "Can Delete Users"},
                 new Permission() { Name= "Roles.Delete",Description = "Can Delete Roles"},
                 new Permission() { Name= "Roles.Create",Description = "Can Create Roles"},
                 new Permission() { Name= "Roles.View",Description = "Can View Roles"},
                 new Permission() { Name= "Reports.View",Description = "Can View Reports"}
               
            };

            // Define role permissions
            var adminPermissions = permissions.Select(p => p.Name).ToList();
            var managerPermissions = new List<string> { "Users.View", "Roles.View", "Reports.View" };
            var financePermissions = new List<string> { "Users.View", "Roles.View", "Reports.View" };
            var userPermissions = new List<string> { "Reports.View" };

            // Associate permissions with Admin User
            foreach (var permissionName in adminPermissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null && !context.UserPermissions.Any(rp => rp.UserId == adminExistingUser.Id && rp.PermissionName == permission.Name))
                {
                    context.UserPermissions.Add(new UserPermission
                    {
                        UserId = adminExistingUser.Id,
                        PermissionName = permission.Name
                    });
                }
            }

            // Associate permissions with roles
            foreach (var permissionName in adminPermissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null && !context.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionName == permission.Name))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = adminRole.Id,
                        PermissionName = permission.Name
                    });
                }
            }


            foreach (var permissionName in managerPermissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null && !context.RolePermissions.Any(rp => rp.RoleId == managerRole.Id && rp.PermissionName == permission.Name))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = managerRole.Id,
                        PermissionName = permission.Name
                    });
                }
            }

            foreach (var permissionName in userPermissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null && !context.RolePermissions.Any(rp => rp.RoleId == userRole.Id && rp.PermissionName == permissionName))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = userRole.Id,
                        PermissionName = permission.Name
                    });
                }
            }
            foreach (var permissionName in financePermissions)
            {
                var permission = permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null && !context.RolePermissions.Any(rp => rp.RoleId == financeRole.Id && rp.PermissionName == permission.Name))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = financeRole.Id,
                        PermissionName = permission.Name
                    });
                }
            }

            await context.SaveChangesAsync();

        }
    }
}
