using MMSA.BLL.Dtos;

namespace MMSA.BLL.Services.Interfaces;

public interface IProgrammingLanguageService
{
    Task<List<ProgrammingLanguageDTO>> GetAllAsync();
    Task<ProgrammingLanguageDTO> CreateAsync(ProgrammingLanguageDTO language);
    Task UpdateAsync(ProgrammingLanguageDTO language);
}
