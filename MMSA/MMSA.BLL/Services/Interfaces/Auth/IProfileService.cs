using MMSA.BLL.Dtos.Auth;

namespace MMSA.BLL.Services.Interfaces.Auth;

public interface IProfileService
{
    Task<ProfileDTO?> GetAsync(string userId);
    Task<IReadOnlyList<string>> UpdateAsync(string userId, UpdateProfileRequestDTO request);
}
