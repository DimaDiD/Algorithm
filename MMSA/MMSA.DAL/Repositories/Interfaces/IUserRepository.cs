namespace MMSA.DAL.Repositories.Interfaces;

public sealed record UserWithRole(string Id, string Email, string? Role, int CourseCount);

public interface IUserRepository
{
    Task<(List<UserWithRole> Items, int Total)> SearchAsync(string? search, int skip, int take);
}
