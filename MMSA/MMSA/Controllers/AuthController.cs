using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.RateLimiting;
using System.Security.Claims;

namespace MMSA.Controllers
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

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            var result = await _authService.RegisterAsync(request);
            return result.Succeeded
                ? Ok(result.Response)
                : BadRequest(new { errors = result.Errors });
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Succeeded
                ? Ok(result.Response)
                : Unauthorized(new { errors = result.Errors });
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                Id = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                Name = User.Identity?.Name,
                Email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
                Roles = User.FindAll("role").Select(c => c.Value)
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequestDTO request)
        {
            var result = await _authService.RefreshAsync(request);
            return result.Succeeded
                ? Ok(result.Response)
                : Unauthorized(new { errors = result.Errors });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequestDTO request)
        {
            await _authService.LogoutAsync(request);
            return NoContent();
        }
    }
}
