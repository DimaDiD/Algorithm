using Microsoft.EntityFrameworkCore;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.DAL.Repositories.Implementations;

public class ProgrammingLanguageRepository : BaseRepository<ProgrammingLanguage>, IProgrammingLanguageRepository
{
    public ProgrammingLanguageRepository(AlgorithmDataContext dataContext) : base(dataContext)
    {
    }

    public Task<List<ProgrammingLanguage>> GetAllOrderedAsync() =>
        _dataContext.ProgrammingLanguages
            .AsNoTracking()
            .OrderBy(language => language.Id)
            .ToListAsync();

    public Task<List<ProgrammingLanguage>> GetByCodesAsync(IReadOnlyCollection<string> codes) =>
        _dataContext.ProgrammingLanguages
            .AsNoTracking()
            .Where(language => codes.Contains(language.Code))
            .ToListAsync();
}
