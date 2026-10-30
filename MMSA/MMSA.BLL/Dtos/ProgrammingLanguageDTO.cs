using System.ComponentModel.DataAnnotations;

namespace MMSA.BLL.Dtos;

public class ProgrammingLanguageDTO
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string HighlightLanguage { get; set; } = string.Empty;
}
