using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reconciliation.Application.DTOs;
using Reconciliation.Application.Extensions;
using Reconciliation.Application.Interfaces.Services;
using Reconciliation.Domain.Entities;
using Reconciliation.Infrastructure.Authorization;
using System.Linq.Expressions;

namespace Reconciliation.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;

        public RolesController( RoleManager<ApplicationRole> roleManager,UserManager<ApplicationUser> userManager,IPermissionService permissionService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _permissionService = permissionService;
        }

        [HttpGet]
        [Permission(Permissions.Roles.View)]
        public async  Task<ActionResult<PaginatedList<RoleDto>>> GetAllRoles([FromQuery] GetPaginatedRoleInput input)
        {
            var efQuery = _roleManager.Roles.Include(p=>p.RolePermissions).AsQueryable();
            // Apply filters using the generic extension
            efQuery = efQuery.ApplyFilters(input, BuildRoleFilterExpressions);
            // Apply sorting using the generic extension
            efQuery = efQuery.ApplySorting(input,
                _ => c => c.Name, null);
            var count = await efQuery.CountAsync();
            var roles = await efQuery.Skip((input.PageIndex - 1) * input.PageSize) .Take(input.PageSize).ToListAsync();

            //var rolesDto = roles.Select(r => new RoleDto
            //{
            //    Id = r.Id,
            //    Name = r.Name,
            //    Description = r.Description,
            //    RolePermissions = r.RolePermissions.Select(rp => new RolePermissionDto
            //    {
            //        Id = rp.Id,
            //        PermissionName = rp.PermissionName
            //    }).ToList()

            //}).ToList();

            var rolesDto = roles.Select(r =>
            {
                // Break circular reference
                foreach (var rp in r.RolePermissions)
                {
                    rp.Role = null;
                }

                return new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    RolePermissions = r.RolePermissions.Select(rp => rp.PermissionName).ToList()
                };
            }).ToList();

            var result = new PaginatedList<RoleDto>(
            rolesDto,
            count,
            input.PageIndex,
            input.PageSize);


            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(Permissions.Roles.View)]
        public async Task<IActionResult> GetRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            // Get permissions for this role
            var permissions = await _permissionService.GetRolePermissionsAsync(id);
          

            return Ok(new
            {
                role.Id,
                role.Name,
                role.Description,
                Permissions = permissions
            });
        }

        [HttpPost]
        [Permission(Permissions.Roles.Create)]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _roleManager.RoleExistsAsync(model.Name))
            {
                return BadRequest("Role already exists");
            }

            var role = new ApplicationRole
            {
                Name = model.Name,
                Description = model.Description
            };

            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, null);
        }

        [HttpPut("{id}")]
        [Permission(Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] RoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Permission(Permissions.Roles.Delete)]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPost("{id}/permissions")]
        [Permission(Permissions.Roles.Edit)]
        public async Task<IActionResult> AddPermissionToRole(string id, [FromBody] PermissionDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var result = await _permissionService.AddPermissionToRoleAsync(id, model.Permission);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok();
        }

        [HttpDelete("{id}/permissions/{permission}")]
        [Permission(Permissions.Roles.Edit)]
        public async Task<IActionResult> RemovePermissionFromRole(string id, string permission)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var result = await _permissionService.RemovePermissionFromRoleAsync(id, permission);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return NoContent();
        }

        [HttpPost("{id}/users")]
        [Permission(Permissions.Users.Edit)]
        public async Task<IActionResult> AddUserToRole(string id, [FromBody] UserRoleDto model)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound("Role not found");
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpDelete("{id}/users/{userId}")]
        [Permission(Permissions.Users.Edit)]
        public async Task<IActionResult> RemoveUserFromRole(string id, string userId)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound("Role not found");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpGet("{id}/users")]
        [Permission(Permissions.Users.View)]
        public async Task<IActionResult> GetUsersInRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var users = await _userManager.GetUsersInRoleAsync(role.Name);

            var userDtos = users.Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllDefinedPermissionsAsync();
            return Ok(permissions);
        }
        [HttpGet("permissionsTree")]
        public async Task<ActionResult<List<PermissionNode>>> GetPermissionsTree()
        {
            var permissions = await _permissionService.GetPermissionsTreeAsync();
            return Ok(permissions);
        }

        private IEnumerable<Expression<Func<ApplicationRole, bool>>> BuildRoleFilterExpressions(GetPaginatedRoleInput input)
        {
            var expressions = new List<Expression<Func<ApplicationRole, bool>>>();
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                expressions.Add(r => r.Name.Contains(input.Filter) || r.Description.Contains(input.Filter));
            }
            return expressions;
        }
    }
}
