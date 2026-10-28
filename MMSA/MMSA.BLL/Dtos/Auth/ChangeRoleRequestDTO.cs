using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Auth;

public class ChangeRoleRequestDTO
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}
