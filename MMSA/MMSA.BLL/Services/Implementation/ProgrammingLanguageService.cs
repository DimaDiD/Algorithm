using System.Text.RegularExpressions;
using MMSA.BLL.Dtos;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class ProgrammingLanguageService : IProgrammingLanguageService
{
    public static readonly IReadOnlySet<string> SupportedHighlightLanguages = new HashSet<string>
    {
        "plaintext", "csharp", "cpp", "c", "delphi", "python", "java", "javascript", "typescript",
        "go", "kotlin", "rust", "php", "ruby", "swift", "sql", "basic", "haskell", "scala", "dart"
    };

    private static readonly Regex CodePattern = new("^[a-z][a-z0-9-]{1,29}$", RegexOptions.Compiled);
    private static readonly string[] ReservedCodes = { "all" };

    private readonly IProgrammingLanguageRepository _languageRepository;

    public ProgrammingLanguageService(IProgrammingLanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    public async Task<List<ProgrammingLanguageDTO>> GetAllAsync()
    {
        var languages = await _languageRepository.GetAllOrderedAsync();
        return languages.Select(ToDto).ToList();
    }

    public async Task<ProgrammingLanguageDTO> CreateAsync(ProgrammingLanguageDTO dto)
    {
        var code = (dto.Code ?? string.Empty).Trim().ToLowerInvariant();

        if (!CodePattern.IsMatch(code) || ReservedCodes.Contains(code))
            throw new ArgumentException("Код мови: 2–30 символів, малі латинські літери, цифри або дефіс, починається з літери.");

        if (await _languageRepository.GetFirstOrDefaultAsync(language => language.Code == code) is not null)
            throw new ArgumentException($"Мова з кодом «{code}» вже існує — оберіть її зі списку.");

        ValidateEditable(dto);

        var language = new ProgrammingLanguage
        {
            Code = code,
            Name = dto.Name.Trim(),
            HighlightLanguage = dto.HighlightLanguage
        };

        await _languageRepository.InsertAsync(language, true);
        return ToDto(language);
    }

    public async Task UpdateAsync(ProgrammingLanguageDTO dto)
    {
        ValidateEditable(dto);

        var language = await _languageRepository.GetFirstOrDefaultAsync(l => l.Id == dto.Id)
            ?? throw new KeyNotFoundException("Мову не знайдено.");

        language.Name = dto.Name.Trim();
        language.HighlightLanguage = dto.HighlightLanguage;

        await _languageRepository.UpdateAsync(language, true);
    }

    private static void ValidateEditable(ProgrammingLanguageDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Trim().Length > 50)
            throw new ArgumentException("Назва мови: від 1 до 50 символів.");

        if (!SupportedHighlightLanguages.Contains(dto.HighlightLanguage))
            throw new ArgumentException("Оберіть підсвітку синтаксису зі списку.");
    }

    internal static ProgrammingLanguageDTO ToDto(ProgrammingLanguage language) => new()
    {
        Id = language.Id,
        Code = language.Code,
        Name = language.Name,
        HighlightLanguage = language.HighlightLanguage
    };
}
