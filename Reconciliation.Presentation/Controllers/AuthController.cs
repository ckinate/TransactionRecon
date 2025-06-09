using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Reconciliation.Application.DTOs;
using Reconciliation.Application.Interfaces.Services;
using Reconciliation.Infrastructure.Services;
using System.Net;
using static Reconciliation.Infrastructure.Authorization.Permissions;

namespace Reconciliation.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(model);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(model);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto model)
        {
            var result = await _authService.RefreshTokenAsync(model.Token, model.RefreshToken);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto model)
        {
            try
            {
                await _authService.RevokeTokenAsync(model.RefreshToken);
                return Ok(new { Message = "Token revoked" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("verify-email")]
        public async Task<ActionResult<AuthResult>> VerifyEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid verification link");
            }
            var result = await _authService.VerifyEmail(userId, token);

            return result;

          
        }

        [HttpPost("resend-verification")]
        public async Task<ActionResult<AuthResult>> ResendVerificationEmail([FromBody] ResendVerificationDto model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest("Please enter your email");
            }

            var result = await _authService.ResendVerificationEmail(model);
            return result;


        }

    }
}
