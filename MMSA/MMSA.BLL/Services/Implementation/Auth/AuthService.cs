using Microsoft.AspNetCore.Identity;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Implementation.Auth
{
    public sealed class AuthService : IAuthService
    {
        private const string InvalidCredentials = "Invalid email or password.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequestDTO request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                return AuthResult.Failure(createResult.Errors.Select(e => e.Description));

            var roleResult = await _userManager.AddToRoleAsync(user, Roles.Reader);
            if (!roleResult.Succeeded)
                return AuthResult.Failure(roleResult.Errors.Select(e => e.Description));

            return AuthResult.Success(CreateResponse(user, new[] { Roles.Reader }));
        }

        public async Task<AuthResult> LoginAsync(LoginRequestDTO request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return AuthResult.Failure(InvalidCredentials);

            var check = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (check.IsLockedOut)
                return AuthResult.Failure("Too many failed attempts. Try again later.");

            if (!check.Succeeded)
                return AuthResult.Failure(InvalidCredentials);

            var roles = await _userManager.GetRolesAsync(user);
            return AuthResult.Success(CreateResponse(user, roles));
        }

        private AuthResponseDTO CreateResponse(ApplicationUser user, IEnumerable<string> roles)
        {
            var (token, expiresAtUtc) = _tokenService.CreateAccessToken(user, roles);
            return new AuthResponseDTO { AccessToken = token, ExpiresAtUtc = expiresAtUtc };
        }
    }
}
