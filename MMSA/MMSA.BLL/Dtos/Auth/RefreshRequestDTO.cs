using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Auth;

public class RefreshRequestDTO
{
    [Required, MaxLength(200)]
    public string RefreshToken { get; set; } = string.Empty;
}