using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MMSA.BLL.Options;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MMSA.BLL.Services.Implementation.Auth
{
    public sealed class TokenService : ITokenService
    {
        private readonly AuthTokenOptions _options;
        private readonly TimeProvider _timeProvider;
        private readonly SigningCredentials _signingCredentials;
        private readonly JsonWebTokenHandler _tokenHandler = new();

        public TokenService(IOptions<AuthTokenOptions> options, TimeProvider timeProvider) 
        {
            _options = options.Value;
            _timeProvider  = timeProvider;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

            _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var expires = now.AddMinutes(_options.AccessTokenMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email ?? String.Empty),
                new(ClaimType.Name, user.UserName ?? String.Empty)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimType.Role, role)));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Subject = new ClaimsIdentity(claims),
                IssuedAt = now,
                NotBefore = now,
                Expires = expires,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(descriptor);
            return (token, expires);
        }

        public string GenerateRefreshToken() =>
            Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

        public string HashRefreshToken(string refreshToken) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
