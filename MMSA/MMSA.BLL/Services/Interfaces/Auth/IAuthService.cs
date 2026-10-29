using MMSA.BLL.Dtos.Auth;

namespace MMSA.BLL.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequestDTO request);
        Task<AuthResult> LoginAsync(LoginRequestDTO request);
        Task<AuthResult> RefreshAsync(RefreshRequestDTO request);
        Task LogoutAsync(RefreshRequestDTO request);
    }
}