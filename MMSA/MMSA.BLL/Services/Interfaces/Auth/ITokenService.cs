using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Interfaces.Auth
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
    }
}
