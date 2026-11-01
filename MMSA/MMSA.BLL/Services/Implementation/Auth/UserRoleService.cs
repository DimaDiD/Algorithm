using Microsoft.AspNetCore.Identity;
using MMSA.BLL.Dtos.Auth;
using MMSA.BLL.Services.Interfaces.Auth;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation.Auth;

public sealed class UserRoleService : IUserRoleService
{
    private const int PageSize = 20;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRepository _userRepository;

    public UserRoleService(UserManager<ApplicationUser> userManager, IUserRepository userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<UserPageDTO> SearchAsync(string? search, int page, string currentUserId)
    {
        page = Math.Max(page, 1);
        var (users, total) = await _userRepository.SearchAsync(search, (page - 1) * PageSize, PageSize);

        return new UserPageDTO
        {
            Page = page,
            PageSize = PageSize,
            Total = total,
            Items = users.Select(user => new UserListItemDTO
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role ?? Roles.Reader,
                CourseCount = user.CourseCount,
                IsCurrentUser = user.Id == currentUserId
            }).ToList()
        };
    }

    public async Task<IReadOnlyList<string>> ChangeRoleAsync(
        ChangeRoleRequestDTO request, string currentUserId)
    {
        if (!Roles.All.Contains(request.Role))
            return new[] { $"Невідома роль «{request.Role}»." };

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new[] { "Користувача не знайдено." };

        if (user.Id == currentUserId)
            return new[] { "Не можна змінити власну роль — попросіть іншого адміністратора." };

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return removeResult.Errors.Select(e => e.Description).ToArray();

        var addResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
            return addResult.Errors.Select(e => e.Description).ToArray();

        await _userManager.UpdateSecurityStampAsync(user);

        return Array.Empty<string>();
    }
}