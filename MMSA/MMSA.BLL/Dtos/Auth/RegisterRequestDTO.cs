using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos.Auth
{
    public class RegisterRequestDTO
    {
        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string Password {  get; set; } = string.Empty;
    }
}
