using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories;
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

        users = users.MatchingSearch(search);

        var total = await users.CountAsync();

        var items = await users
            .OrderByName()
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
                _dataContext.Courses.Count(course =>
                    course.OwnerId == user.Id || course.Teachers.Any(teacher => teacher.UserId == user.Id))))
            .ToListAsync();

        return (items, total);
    }
}
