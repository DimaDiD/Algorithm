using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.RateLimiting;
using System.Security.Claims;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string RefreshCookieName = "mmsa_refresh";
        private const string RefreshCookiePath = "/api/Auth";

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
                ? TokenResponse(result.Response!)
                : BadRequest(new { errors = result.Errors });
        }

        [EnableRateLimiting(RateLimitPolicies.Auth)]
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDTO request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Succeeded
                ? TokenResponse(result.Response!)
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
                Roles = User.FindAll(ClaimType.Role).Select(c => c.Value)
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshCookieName];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { errors = new[] { "Refresh token is missing." } });

            var result = await _authService.RefreshAsync(new RefreshRequestDTO { RefreshToken = refreshToken });
            if (!result.Succeeded)
            {
                // Токен недійсний (прострочений, анульований, reuse detection) - прибираємо його з браузера.
                DeleteRefreshCookie();
                return Unauthorized(new { errors = result.Errors });
            }

            return TokenResponse(result.Response!);
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies[RefreshCookieName];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.LogoutAsync(new RefreshRequestDTO { RefreshToken = refreshToken });

            DeleteRefreshCookie();
            return NoContent();
        }

        private IActionResult TokenResponse(AuthResponseDTO response)
        {
            Response.Cookies.Append(RefreshCookieName, response.RefreshToken, CreateRefreshCookieOptions(
                new DateTimeOffset(response.RefreshTokenExpiresAtUtc, TimeSpan.Zero)));

            return Ok(new AccessTokenResponseDTO
            {
                AccessToken = response.AccessToken,
                TokenType = response.TokenType,
                ExpiresAtUtc = response.ExpiresAtUtc
            });
        }

        private void DeleteRefreshCookie() =>
            Response.Cookies.Delete(RefreshCookieName, CreateRefreshCookieOptions(expires: null));

        private static CookieOptions CreateRefreshCookieOptions(DateTimeOffset? expires) => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = RefreshCookiePath,
            Expires = expires
        };
    }
}
