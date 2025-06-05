using Microsoft.AspNetCore.Authorization;
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
using System.Security.Claims;
using static Reconciliation.Infrastructure.Authorization.Permissions;

namespace Reconciliation.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;

        public UsersController( UserManager<ApplicationUser> userManager,IPermissionService permissionService)
        {
            _userManager = userManager;
            _permissionService = permissionService;
        }

        [HttpGet]
        [Permission(Permissions.Users.View)]
        public async Task<ActionResult<PaginatedList<ApplicationUserDto>>> GetAllUsers([FromQuery] GetPaginatedUserInput input)
        {
            var efQuery =  _userManager.Users.AsQueryable();
            // Apply filters using the generic extension
            efQuery = efQuery.ApplyFilters(input, BuildUserFilterExpressions);
            // Apply sorting using the generic extension
            efQuery = efQuery.ApplySorting(input,
                _ => c => c.FirstName, null);
            var count = await efQuery.CountAsync();
            var users = await efQuery.Skip((input.PageIndex - 1) * input.PageSize).Take(input.PageSize).ToListAsync();
            var usersDto = users.Select(r => new ApplicationUserDto
            {
                Id = r.Id,
                FirstName = r.FirstName,
                LastName = r.LastName,
                Email = r.Email,
                UserName = r.UserName,
                IsActive = r.IsActive,
                LastLoginAt = r.LastLoginAt,

            }).ToList();

            var result = new PaginatedList<ApplicationUserDto>(
            usersDto,
            count,
            input.PageIndex,
            input.PageSize);
           

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(Permissions.Users.View)]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetUserPermissionsAsync(id);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.LastLoginAt,
                Role = roles.FirstOrDefault(),
                Permissions = permissions
            });
        }

        [HttpGet("get-current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                Roles = roles,
                Permissions = permissions
            });
        }

        [HttpPost]
        [Permission(Permissions.Users.Create)]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!string.IsNullOrEmpty(model.Role))
            {
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, null);
        }

        [HttpPut("{id}")]
        [Permission(Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsActive = model.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Permission(Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Instead of permanent deletion, consider soft delete
            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost("{id}/permissions")]
        [Permission(Permissions.Users.Edit)]
        public async Task<IActionResult> AddPermissionToUser(string id, [FromBody] PermissionDto model)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _permissionService.AddPermissionToUserAsync(id, model.Permission);

            if (!result.Success)
            {
                return BadRequest("Permission already exists for this user");
            }

            return Ok();
        }
        [HttpDelete("{id}/permissions/{permission}")]
        [Permission(Permissions.Users.Edit)]
        public async Task<IActionResult> RemovePermissionFromUser(string id, string permission)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _permissionService.RemovePermissionFromUserAsync(id, permission);

            if (!result.Success)
            {
                return BadRequest("Permission not found for this user");
            }

            return NoContent();
        }

        private IEnumerable<Expression<Func<ApplicationUser, bool>>> BuildUserFilterExpressions(GetPaginatedUserInput input)
        {
            var expressions = new List<Expression<Func<ApplicationUser, bool>>>();
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                expressions.Add(r => r.FirstName.Contains(input.Filter) || r.LastName.Contains(input.Filter) );
            }
            return expressions;
        }
    }
}
