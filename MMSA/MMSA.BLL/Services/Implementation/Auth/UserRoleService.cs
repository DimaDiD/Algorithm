using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private readonly IUnitOfWork _unitOfWork;

    public UserRoleService(UserManager<ApplicationUser> userManager, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
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

        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(request.Role) && currentRoles.Count == 1)
                return Array.Empty<string>();

            if (currentRoles.Contains(Roles.Admin) && request.Role != Roles.Admin
                && (await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count <= 1)
                return new[] { "Це останній адміністратор — спочатку призначте іншого." };

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return removeResult.Errors.Select(e => e.Description).ToArray();

            var addResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addResult.Succeeded)
                return addResult.Errors.Select(e => e.Description).ToArray();

            var stampResult = await _userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
                return stampResult.Errors.Select(e => e.Description).ToArray();

            await transaction.CommitAsync();
            return Array.Empty<string>();
        }
        catch (DbUpdateException)
        {
            return new[] { "Роль одночасно змінює інший адміністратор. Оновіть сторінку й спробуйте ще раз." };
        }
    }
}