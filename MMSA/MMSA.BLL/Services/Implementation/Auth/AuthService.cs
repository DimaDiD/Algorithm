using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Options;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation.Auth;

public sealed class AuthService : IAuthService
{
    private const string InvalidCredentials = "Invalid email or password.";
    private const string InvalidRefreshToken = "Invalid or expired refresh token.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly AuthTokenOptions _authTokenOptions;
    private readonly TimeProvider _timeProvider;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<AuthTokenOptions> authTokenOptions,
        TimeProvider timeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _authTokenOptions = authTokenOptions.Value;
        _timeProvider = timeProvider;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequestDTO request)
    {
        var user = new ApplicationUser { UserName = request.Email, Email = request.Email };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return AuthResult.Failure(createResult.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, Roles.Reader);
        if (!roleResult.Succeeded)
            return AuthResult.Failure(roleResult.Errors.Select(e => e.Description));

        var response = await IssueTokensAsync(user, new[] { Roles.Reader }, Guid.NewGuid());
        return AuthResult.Success(response);
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
        var response = await IssueTokensAsync(user, roles, Guid.NewGuid());
        return AuthResult.Success(response);
    }

    public async Task<AuthResult> RefreshAsync(RefreshRequestDTO request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetFirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (stored is null)
            return AuthResult.Failure(InvalidRefreshToken);

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (stored.RevokedAtUtc is not null)
        {
            await RevokeFamilyAsync(stored.FamilyId, now);
            return AuthResult.Failure(InvalidRefreshToken);
        }

        if (stored.ExpiresAtUtc <= now)
            return AuthResult.Failure(InvalidRefreshToken);

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user is null || user.SecurityStamp != stored.SecurityStamp)
        {
            await RevokeFamilyAsync(stored.FamilyId, now);
            return AuthResult.Failure(InvalidRefreshToken);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = await IssueTokensAsync(user, roles, stored.FamilyId, replacedToken: stored);
        return AuthResult.Success(response);
    }

    public async Task LogoutAsync(RefreshRequestDTO request)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetFirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (stored is null)
            return;

        await RevokeFamilyAsync(stored.FamilyId, _timeProvider.GetUtcNow().UtcDateTime);
    }

    private async Task<AuthResponseDTO> IssueTokensAsync(
        ApplicationUser user,
        IEnumerable<string> roles,
        Guid familyId,
        RefreshToken? replacedToken = null)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var (accessToken, accessExpiresAtUtc) = _tokenService.CreateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            FamilyId = familyId,
            SecurityStamp = user.SecurityStamp,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_authTokenOptions.RefreshTokenDays)
        };
        await _refreshTokenRepository.InsertAsync(entity, isSave: false);

        if (replacedToken is not null)
        {
            replacedToken.RevokedAtUtc = now;
            replacedToken.ReplacedByTokenId = entity.Id;
            await _refreshTokenRepository.UpdateAsync(replacedToken, isSave: false);
        }

        await _refreshTokenRepository.SaveAsync();

        return new AuthResponseDTO
        {
            AccessToken = accessToken,
            ExpiresAtUtc = accessExpiresAtUtc,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAtUtc = entity.ExpiresAtUtc
        };
    }

    private async Task RevokeFamilyAsync(Guid familyId, DateTime now)
    {
        var activeTokens = await _refreshTokenRepository.GetAllAsync(
            t => t.FamilyId == familyId && t.RevokedAtUtc == null);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = now;
            await _refreshTokenRepository.UpdateAsync(token, isSave: false);
        }

        await _refreshTokenRepository.SaveAsync();
    }
}