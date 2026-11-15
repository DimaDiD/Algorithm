using Microsoft.AspNetCore.Identity;

namespace MMSA.DAL.Entities;

public class ApplicationUser : IdentityUser
{
    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }
}
