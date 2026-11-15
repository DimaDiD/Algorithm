namespace MMSA.BLL.Dtos.Auth;

public class ProfileDTO
{
    public string Email { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
}

public class UpdateProfileRequestDTO
{
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
}
