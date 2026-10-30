using MMSA.DAL.Entities;

namespace MMSA.DAL.Repositories.Interfaces;

public interface IProgrammingLanguageRepository : IBaseRepository<ProgrammingLanguage>
{
    Task<List<ProgrammingLanguage>> GetAllOrderedAsync();
    Task<List<ProgrammingLanguage>> GetByCodesAsync(IReadOnlyCollection<string> codes);
}
