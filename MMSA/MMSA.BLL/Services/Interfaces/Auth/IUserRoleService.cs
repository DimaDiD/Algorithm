using MMSA.BLL.Dtos.Auth;

namespace MMSA.BLL.Services.Interfaces.Auth
{
    public interface IUserRoleService
    {
        Task<IReadOnlyList<string>> ChangeRoleAsync(ChangeRoleRequestDTO request, string currentUserId);
        Task<UserPageDTO> SearchAsync(string? search, int page, string currentUserId);
    }
}
