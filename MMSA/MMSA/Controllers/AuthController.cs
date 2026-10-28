using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDTO request)
        {
            var result = await _authService.RegisterAsync(request);
            return result.Succeeded
                ? Ok(result.Response)
                : BadRequest(new { errors = result.Errors });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Succeeded
                ? Ok(result.Response)
                : Unauthorized(new { errors = result.Errors });
        }

        [Authorize]
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
    }
}
