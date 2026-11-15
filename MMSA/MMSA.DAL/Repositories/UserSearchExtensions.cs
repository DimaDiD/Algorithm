using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories;

public static class UserSearchExtensions
{
    private const int MaxTerms = 5;

    public static IQueryable<ApplicationUser> MatchingSearch(this IQueryable<ApplicationUser> users, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return users;

        var terms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(MaxTerms);
        foreach (var term in terms)
        {
            var upperTerm = term.ToUpperInvariant();
            users = users.Where(user =>
                (user.NormalizedEmail != null && user.NormalizedEmail.Contains(upperTerm)) ||
                (user.LastName != null && user.LastName.Contains(term)) ||
                (user.FirstName != null && user.FirstName.Contains(term)) ||
                (user.MiddleName != null && user.MiddleName.Contains(term)));
        }

        return users;
    }

    public static IOrderedQueryable<ApplicationUser> OrderByName(this IQueryable<ApplicationUser> users) =>
        users
            .OrderBy(user => user.LastName == null)
            .ThenBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.Email);
}
