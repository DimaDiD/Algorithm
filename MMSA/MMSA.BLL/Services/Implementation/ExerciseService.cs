using System.Text.RegularExpressions;
using MMSA.BLL.Dtos.Exercises;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class ExerciseService : IExerciseService
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    private readonly IExerciseRepository _exerciseRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly TimeProvider _timeProvider;

    public ExerciseService(
        IExerciseRepository exerciseRepository,
        ICourseRepository courseRepository,
        TimeProvider timeProvider)
    {
        _exerciseRepository = exerciseRepository;
        _courseRepository = courseRepository;
        _timeProvider = timeProvider;
    }

    public async Task<List<ExerciseDTO>> GetForTopicAsync(int pageId, int? subPageId)
    {
        var exercises = await _exerciseRepository.GetByTopicAsync(pageId, subPageId);
        return exercises.Select(ToStudentDto).ToList();
    }

    public async Task<ExerciseEditDTO?> GetForEditAsync(int id)
    {
        var exercise = await _exerciseRepository.GetWithQuestionsAsync(id);
        return exercise is null ? null : ToEditDto(exercise);
    }

    public async Task<int> CreateAsync(ExerciseEditDTO dto)
    {
        var access = await _courseRepository.GetAccessForTopicAsync(dto.PageId, dto.SubPageId)
            ?? throw new KeyNotFoundException("Тему не знайдено.");
        Validate(dto, await _courseRepository.GetLanguageCodesAsync(access.CourseId));

        var exercise = new Exercise
        {
            PageId = dto.PageId,
            SubPageId = dto.SubPageId,
            Title = dto.Title.Trim(),
            Description = NullIfEmpty(dto.Description),
            Order = await _exerciseRepository.GetNextOrderAsync(dto.PageId, dto.SubPageId),
            CreatedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
        };

        for (var index = 0; index < dto.Questions.Count; index++)
        {
            var question = new Question();
            ApplyQuestion(question, dto.Questions[index], index);
            exercise.Questions.Add(question);
        }

        await _exerciseRepository.InsertAsync(exercise, true);
        return exercise.Id;
    }

    public async Task UpdateAsync(ExerciseEditDTO dto)
    {
        var access = await _courseRepository.GetAccessForExerciseAsync(dto.Id)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");
        Validate(dto, await _courseRepository.GetLanguageCodesAsync(access.CourseId));

        var exercise = await _exerciseRepository.GetWithQuestionsAsync(dto.Id, track: true)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");

        exercise.Title = dto.Title.Trim();
        exercise.Description = NullIfEmpty(dto.Description);

        var incomingIds = dto.Questions.Where(q => q.Id != 0).Select(q => q.Id).ToHashSet();
        foreach (var removed in exercise.Questions.Where(q => !incomingIds.Contains(q.Id)).ToList())
            exercise.Questions.Remove(removed);

        for (var index = 0; index < dto.Questions.Count; index++)
        {
            var questionDto = dto.Questions[index];
            var question = questionDto.Id == 0
                ? null
                : exercise.Questions.FirstOrDefault(q => q.Id == questionDto.Id);

            if (question is null)
            {
                question = new Question();
                exercise.Questions.Add(question);
            }

            ApplyQuestion(question, questionDto, index);
        }

        await _exerciseRepository.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var exercise = await _exerciseRepository.GetFirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");

        await _exerciseRepository.DeleteAsync(exercise, true);
    }

    public async Task<ExerciseCheckResultDTO> CheckAsync(int id, ExerciseCheckRequestDTO request)
    {
        var exercise = await _exerciseRepository.GetWithQuestionsAsync(id)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");

        var answers = request.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.First());

        var result = new ExerciseCheckResultDTO();

        foreach (var question in exercise.Questions.OrderBy(q => q.Order))
        {
            answers.TryGetValue(question.Id, out var answer);
            var questionResult = CheckQuestion(question, answer);
            result.Questions.Add(questionResult);

            if (questionResult.Status != QuestionResultStatus.NotChecked)
            {
                result.MaxScore += questionResult.MaxPoints;
                result.Score += questionResult.EarnedPoints;
            }
        }

        return result;
    }

    private static QuestionResultDTO CheckQuestion(Question question, QuestionAnswerDTO? answer)
    {
        var result = new QuestionResultDTO { QuestionId = question.Id, MaxPoints = question.Points };
        var correctOptions = question.Options.Where(o => o.IsCorrect).ToList();

        bool isCorrect;
        switch (question.Type)
        {
            case QuestionType.SingleChoice:
            case QuestionType.MultipleChoice:
                var expected = correctOptions.Select(o => o.Id).ToHashSet();
                var selected = (answer?.SelectedOptionIds ?? new List<int>()).ToHashSet();
                isCorrect = expected.SetEquals(selected);
                result.CorrectOptionIds = expected.ToList();
                break;

            case QuestionType.ShortAnswer:
                var given = NormalizeShortAnswer(answer?.Text);
                isCorrect = given.Length > 0 && correctOptions.Any(o => NormalizeShortAnswer(o.Text) == given);
                result.AcceptedAnswers = correctOptions.OrderBy(o => o.Order).Select(o => o.Text).ToList();
                break;

            default:
                result.Status = QuestionResultStatus.NotChecked;
                return result;
        }

        result.Status = isCorrect ? QuestionResultStatus.Correct : QuestionResultStatus.Incorrect;
        result.EarnedPoints = isCorrect ? question.Points : 0;
        return result;
    }

    private static string NormalizeShortAnswer(string? value) =>
        Whitespace.Replace((value ?? string.Empty).Trim(), " ")
            .Replace(',', '.')
            .ToLowerInvariant();

    private static void ApplyQuestion(Question question, QuestionEditDTO dto, int order)
    {
        var type = Enum.Parse<QuestionType>(dto.Type);

        question.Type = type;
        question.Text = dto.Text.Trim();
        question.Points = dto.Points;
        question.Order = order;
        question.CodeLanguage = type == QuestionType.Code ? dto.CodeLanguage : null;
        question.ReferenceAnswer = type is QuestionType.OpenAnswer or QuestionType.Code ? NullIfEmpty(dto.ReferenceAnswer) : null;
        question.Rubric = type is QuestionType.OpenAnswer or QuestionType.Code ? NullIfEmpty(dto.Rubric) : null;

        var optionDtos = HasOptions(type) ? dto.Options : new List<QuestionOptionEditDTO>();

        var incomingIds = optionDtos.Where(o => o.Id != 0).Select(o => o.Id).ToHashSet();
        foreach (var removed in question.Options.Where(o => !incomingIds.Contains(o.Id)).ToList())
            question.Options.Remove(removed);

        for (var index = 0; index < optionDtos.Count; index++)
        {
            var optionDto = optionDtos[index];
            var option = optionDto.Id == 0 ? null : question.Options.FirstOrDefault(o => o.Id == optionDto.Id);
            if (option is null)
            {
                option = new QuestionOption();
                question.Options.Add(option);
            }

            option.Text = optionDto.Text.Trim();
            option.IsCorrect = type == QuestionType.ShortAnswer || optionDto.IsCorrect;
            option.Order = index;
        }
    }

    private static void Validate(ExerciseEditDTO dto, IReadOnlySet<string> codeLanguages)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Вкажіть назву завдання.");

        if (dto.Questions.Count == 0)
            throw new ArgumentException("Додайте хоча б одне питання.");

        for (var index = 0; index < dto.Questions.Count; index++)
        {
            var question = dto.Questions[index];
            var label = $"Питання {index + 1}";

            if (!Enum.TryParse<QuestionType>(question.Type, out var type) || !Enum.IsDefined(type))
                throw new ArgumentException($"{label}: невідомий тип питання.");

            if (string.IsNullOrWhiteSpace(question.Text))
                throw new ArgumentException($"{label}: введіть текст питання.");

            var options = question.Options.Where(o => !string.IsNullOrWhiteSpace(o.Text)).ToList();
            if (options.Count != question.Options.Count)
                throw new ArgumentException($"{label}: варіант відповіді не може бути порожнім.");

            switch (type)
            {
                case QuestionType.SingleChoice:
                    if (options.Count < 2)
                        throw new ArgumentException($"{label}: потрібно щонайменше 2 варіанти.");
                    if (options.Count(o => o.IsCorrect) != 1)
                        throw new ArgumentException($"{label}: позначте рівно один правильний варіант.");
                    break;

                case QuestionType.MultipleChoice:
                    if (options.Count < 2)
                        throw new ArgumentException($"{label}: потрібно щонайменше 2 варіанти.");
                    if (!options.Any(o => o.IsCorrect))
                        throw new ArgumentException($"{label}: позначте хоча б один правильний варіант.");
                    break;

                case QuestionType.ShortAnswer:
                    if (options.Count == 0)
                        throw new ArgumentException($"{label}: вкажіть хоча б одну правильну відповідь.");
                    break;

                case QuestionType.Code:
                    if (question.CodeLanguage is null || !codeLanguages.Contains(question.CodeLanguage))
                        throw new ArgumentException($"{label}: оберіть мову програмування.");
                    break;
            }
        }
    }

    private static bool HasOptions(QuestionType type) =>
        type is QuestionType.SingleChoice or QuestionType.MultipleChoice or QuestionType.ShortAnswer;

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ExerciseDTO ToStudentDto(Exercise exercise) => new()
    {
        Id = exercise.Id,
        Title = exercise.Title,
        Description = exercise.Description,
        Order = exercise.Order,
        Questions = exercise.Questions
            .OrderBy(q => q.Order)
            .Select(q => new QuestionDTO
            {
                Id = q.Id,
                Type = q.Type.ToString(),
                Text = q.Text,
                Points = q.Points,
                Order = q.Order,
                CodeLanguage = q.CodeLanguage,
                Options = q.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice
                    ? q.Options.OrderBy(o => o.Order).Select(o => new QuestionOptionDTO { Id = o.Id, Text = o.Text }).ToList()
                    : new List<QuestionOptionDTO>()
            })
            .ToList()
    };

    private static ExerciseEditDTO ToEditDto(Exercise exercise) => new()
    {
        Id = exercise.Id,
        PageId = exercise.PageId,
        SubPageId = exercise.SubPageId,
        Title = exercise.Title,
        Description = exercise.Description,
        Order = exercise.Order,
        Questions = exercise.Questions
            .OrderBy(q => q.Order)
            .Select(q => new QuestionEditDTO
            {
                Id = q.Id,
                Type = q.Type.ToString(),
                Text = q.Text,
                Points = q.Points,
                CodeLanguage = q.CodeLanguage,
                ReferenceAnswer = q.ReferenceAnswer,
                Rubric = q.Rubric,
                Options = q.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new QuestionOptionEditDTO { Id = o.Id, Text = o.Text, IsCorrect = o.IsCorrect })
                    .ToList()
            })
            .ToList()
    };
}
