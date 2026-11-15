using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AlgorithmDataContext _dataContext;

    public UserRepository(AlgorithmDataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<(List<UserWithRole> Items, int Total)> SearchAsync(string? search, int skip, int take)
    {
        var users = _dataContext.Users.AsNoTracking();

        foreach (var term in SplitTerms(search))
        {
            var upperTerm = term.ToUpperInvariant();
            users = users.Where(user =>
                (user.NormalizedEmail != null && user.NormalizedEmail.Contains(upperTerm)) ||
                (user.LastName != null && user.LastName.Contains(term)) ||
                (user.FirstName != null && user.FirstName.Contains(term)) ||
                (user.MiddleName != null && user.MiddleName.Contains(term)));
        }

        var total = await users.CountAsync();

        var items = await users
            .OrderBy(user => user.LastName == null)
            .ThenBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email)
            .Skip(skip)
            .Take(take)
            .Select(user => new UserWithRole(
                user.Id,
                user.Email ?? string.Empty,
                user.LastName,
                user.FirstName,
                user.MiddleName,
                (from userRole in _dataContext.UserRoles
                 join role in _dataContext.Roles on userRole.RoleId equals role.Id
                 where userRole.UserId == user.Id
                 select role.Name).FirstOrDefault(),
                _dataContext.Courses.Count(course => course.OwnerId == user.Id)))
            .ToListAsync();

        return (items, total);
    }

    private static IEnumerable<string> SplitTerms(string? search) =>
        string.IsNullOrWhiteSpace(search)
            ? Enumerable.Empty<string>()
            : search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(5);
}
