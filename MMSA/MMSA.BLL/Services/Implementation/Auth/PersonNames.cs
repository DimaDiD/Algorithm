using System.Text.RegularExpressions;
using MMSA.DAL.Entities;

namespace MMSA.BLL.Services.Implementation.Auth;

public static class PersonNames
{
    public const int MaxLength = 50;

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Whitespace.Replace(value.Trim(), " ");

    public static IReadOnlyList<string> Validate(string? lastName, string? firstName, string? middleName)
    {
        var errors = new List<string>();

        if (lastName is null)
            errors.Add("Вкажіть прізвище.");
        if (firstName is null)
            errors.Add("Вкажіть ім'я.");

        if (lastName?.Length > MaxLength || firstName?.Length > MaxLength || middleName?.Length > MaxLength)
            errors.Add($"Прізвище, ім'я та по батькові — не довше {MaxLength} символів кожне.");

        return errors;
    }

    public static string? ShortName(ApplicationUser user) =>
        user.FirstName is null && user.LastName is null
            ? null
            : string.Join(" ", new[] { user.FirstName, user.LastName }.Where(part => part is not null));
}
