using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MMSA.BLL.Dtos;
using MMSA.BLL.Dtos.Exercises;
using MMSA.BLL.Options;
using MMSA.BLL.Services.Interfaces;
using MMSA.BLL.Services.Interfaces.Ai;
using MMSA.DAL.Constants;
using MMSA.DAL.Entities;
using MMSA.DAL.Repositories.Interfaces;

namespace MMSA.BLL.Services.Implementation;

public sealed class ExerciseService : IExerciseService
{
    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);
    private const int StoredAnswerMaxLength = 4000;
    private const int TeacherFieldMaxLength = 4000;
    private const int TopWrongAnswers = 5;

    private readonly IExerciseRepository _exerciseRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IProgrammingLanguageRepository _languageRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IAiReviewService _aiReviewService;
    private readonly IAiUsageLimiter _aiUsageLimiter;
    private readonly IMemoryCache _cache;
    private readonly AiReviewOptions _aiOptions;
    private readonly TimeProvider _timeProvider;

    public ExerciseService(
        IExerciseRepository exerciseRepository,
        ICourseRepository courseRepository,
        ISubmissionRepository submissionRepository,
        IProgrammingLanguageRepository languageRepository,
        IReviewRepository reviewRepository,
        IAiReviewService aiReviewService,
        IAiUsageLimiter aiUsageLimiter,
        IMemoryCache cache,
        IOptions<AiReviewOptions> aiOptions,
        TimeProvider timeProvider)
    {
        _exerciseRepository = exerciseRepository;
        _courseRepository = courseRepository;
        _submissionRepository = submissionRepository;
        _languageRepository = languageRepository;
        _reviewRepository = reviewRepository;
        _aiReviewService = aiReviewService;
        _aiUsageLimiter = aiUsageLimiter;
        _cache = cache;
        _aiOptions = aiOptions.Value;
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

    public async Task MoveAsync(int id, MoveDirection direction)
    {
        var target = await _exerciseRepository.GetFirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");

        var siblings = (await _exerciseRepository.GetAllAsync(e => e.PageId == target.PageId && e.SubPageId == target.SubPageId))
            .OrderBy(e => e.Order)
            .ThenBy(e => e.Id)
            .ToList();

        var index = siblings.FindIndex(e => e.Id == id);
        var neighbourIndex = direction == MoveDirection.Up ? index - 1 : index + 1;
        if (neighbourIndex < 0 || neighbourIndex >= siblings.Count)
            return;

        (siblings[index], siblings[neighbourIndex]) = (siblings[neighbourIndex], siblings[index]);

        for (var position = 0; position < siblings.Count; position++)
        {
            if (siblings[position].Order == position)
                continue;

            siblings[position].Order = position;
            await _exerciseRepository.UpdateAsync(siblings[position], false);
        }

        await _exerciseRepository.SaveAsync();
    }

    public AiStatusDTO GetAiStatus() => new()
    {
        Enabled = _aiReviewService.IsEnabled,
        MaxAnswerLength = _aiOptions.MaxAnswerLength,
        MaxCodeLength = _aiOptions.MaxCodeLength
    };

    public async Task<ExerciseCheckResultDTO> CheckAsync(
        int id,
        ExerciseCheckRequestDTO request,
        ExerciseCheckContext context,
        CancellationToken cancellationToken)
    {
        var exercise = await _exerciseRepository.GetWithQuestionsAsync(id)
            ?? throw new KeyNotFoundException("Завдання не знайдено.");

        var answers = request.Answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.First());

        var questions = exercise.Questions.OrderBy(q => q.Order).ToList();
        ValidateAiAnswerLengths(questions, answers);

        var languageNames = questions.Any(q => q.Type == QuestionType.Code)
            ? (await _languageRepository.GetAllOrderedAsync()).ToDictionary(l => l.Code, l => l.Name)
            : new Dictionary<string, string>();

        var aiReviews = questions
            .Where(q => q.Type is QuestionType.OpenAnswer or QuestionType.Code)
            .ToDictionary(
                q => q.Id,
                q => GradeWithAiAsync(q, answers.GetValueOrDefault(q.Id), languageNames, context.ClientKey, cancellationToken));

        var result = new ExerciseCheckResultDTO();

        foreach (var question in questions)
        {
            answers.TryGetValue(question.Id, out var answer);
            var questionResult = aiReviews.TryGetValue(question.Id, out var review)
                ? await review
                : CheckQuestion(question, answer);
            result.Questions.Add(questionResult);

            if (questionResult.Status != QuestionResultStatus.NotChecked)
            {
                result.MaxScore += questionResult.MaxPoints;
                result.Score += questionResult.EarnedPoints;
            }
        }

        if (context.RecordSubmission && request.ClientId is { } clientId && clientId != Guid.Empty)
            await SaveSubmissionAsync(exercise, clientId, answers, result);

        return result;
    }

    public async Task<List<ExerciseProgressDTO>> GetProgressAsync(int pageId, int? subPageId, Guid clientId)
    {
        if (clientId == Guid.Empty)
            return new List<ExerciseProgressDTO>();

        var exerciseIds = (await _exerciseRepository.GetByTopicAsync(pageId, subPageId)).Select(e => e.Id).ToList();
        var submissions = await _submissionRepository.GetForClientAsync(clientId, exerciseIds);
        var reviews = (await _reviewRepository.GetReviewedAnswersForClientAsync(clientId, exerciseIds))
            .ToLookup(answer => answer.Submission.ExerciseId);

        return submissions
            .GroupBy(s => s.ExerciseId)
            .Select(group =>
            {
                var last = group.MaxBy(s => s.SubmittedAtUtc)!;
                return new ExerciseProgressDTO
                {
                    ExerciseId = group.Key,
                    Attempts = group.Count(),
                    BestScore = group.Max(s => s.Score),
                    LastScore = last.Score,
                    MaxScore = last.MaxScore,
                    LastSubmittedAtUtc = last.SubmittedAtUtc,
                    TeacherReviews = reviews[group.Key]
                        .GroupBy(answer => answer.QuestionId)
                        .Select(answers => answers.MaxBy(answer => answer.ReviewedAtUtc)!)
                        .Select(answer => new TeacherReviewDTO
                        {
                            QuestionId = answer.QuestionId,
                            Points = answer.EarnedPoints,
                            MaxPoints = answer.MaxPoints,
                            IsOverridden = answer.ReviewStatus == ReviewStatuses.Overridden,
                            Comment = answer.TeacherComment,
                            ReviewedAtUtc = answer.ReviewedAtUtc!.Value
                        })
                        .ToList()
                };
            })
            .ToList();
    }

    public async Task<ExerciseStatsDTO?> GetStatsAsync(int id)
    {
        var exercise = await _exerciseRepository.GetWithQuestionsAsync(id);
        if (exercise is null)
            return null;

        var submissions = await _submissionRepository.GetForExerciseWithAnswersAsync(id);
        var firstAttempts = submissions.GroupBy(s => s.ClientId).Select(g => g.First()).ToList();
        var bestAttempts = submissions.GroupBy(s => s.ClientId).Select(g => g.MaxBy(Percent)!).ToList();
        var answersByQuestion = submissions
            .SelectMany(s => s.Answers)
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new ExerciseStatsDTO
        {
            ExerciseId = exercise.Id,
            Title = exercise.Title,
            Attempts = submissions.Count,
            Students = firstAttempts.Count,
            AveragePercent = AveragePercent(submissions),
            FirstAttemptAveragePercent = AveragePercent(firstAttempts),
            BestAveragePercent = AveragePercent(bestAttempts),
            LastSubmittedAtUtc = submissions.Count == 0 ? null : submissions.Max(s => s.SubmittedAtUtc),
            Questions = exercise.Questions
                .OrderBy(q => q.Order)
                .Select(q => BuildQuestionStats(q, answersByQuestion.GetValueOrDefault(q.Id) ?? new List<SubmissionAnswer>()))
                .ToList()
        };
    }

    private void ValidateAiAnswerLengths(IEnumerable<Question> questions, IReadOnlyDictionary<int, QuestionAnswerDTO> answers)
    {
        var index = 0;
        foreach (var question in questions)
        {
            index++;
            if (question.Type is not (QuestionType.OpenAnswer or QuestionType.Code) || !answers.TryGetValue(question.Id, out var answer))
                continue;

            var limit = question.Type == QuestionType.Code ? _aiOptions.MaxCodeLength : _aiOptions.MaxAnswerLength;
            if (PrepareAiAnswer(question, answer.Text).Length > limit)
                throw new ArgumentException($"Питання {index}: відповідь задовга (максимум {limit} символів).");
        }
    }

    private static string PrepareAiAnswer(Question question, string? text) =>
        question.Type == QuestionType.Code
            ? (text ?? string.Empty).Replace("\r\n", "\n").TrimEnd()
            : (text ?? string.Empty).Trim();

    private async Task<QuestionResultDTO> GradeWithAiAsync(
        Question question,
        QuestionAnswerDTO? answer,
        IReadOnlyDictionary<string, string> languageNames,
        string clientKey,
        CancellationToken cancellationToken)
    {
        var result = new QuestionResultDTO { QuestionId = question.Id, MaxPoints = question.Points };
        var text = PrepareAiAnswer(question, answer?.Text);
        var isCode = question.Type == QuestionType.Code;

        if (string.IsNullOrWhiteSpace(text))
        {
            result.Status = QuestionResultStatus.Incorrect;
            return result;
        }

        if (isCode && text == PrepareAiAnswer(question, question.StarterCode))
        {
            result.Status = QuestionResultStatus.Incorrect;
            result.Feedback = "Ви ще не змінили заготовку коду — допишіть рішення й перевірте знову.";
            return result;
        }

        if (!_aiReviewService.IsEnabled)
            return NotChecked(result, "AI-перевірка зараз вимкнена. Порівняйте свою відповідь із матеріалом теми.");

        var cacheKey = BuildAiCacheKey(question, text);
        if (!_cache.TryGetValue(cacheKey, out AiReviewVerdict? verdict) || verdict is null)
        {
            if (!_aiUsageLimiter.TryAcquire(clientKey))
                return NotChecked(result, "Ліміт AI-перевірок на сьогодні вичерпано. Спробуйте завтра.");

            var languageName = isCode
                ? languageNames.GetValueOrDefault(question.CodeLanguage ?? string.Empty) ?? question.CodeLanguage ?? "unknown"
                : null;

            verdict = await _aiReviewService.ReviewAsync(
                new AiReviewRequest(question.Text, question.ReferenceAnswer, question.Rubric, question.Points, text, languageName),
                cancellationToken);

            if (verdict is null)
                return NotChecked(result, "AI-перевірка тимчасово недоступна. Спробуйте трохи пізніше.");

            _cache.Set(cacheKey, verdict, TimeSpan.FromHours(_aiOptions.CacheHours));
        }

        result.IsAiReviewed = true;
        result.EarnedPoints = verdict.Points;
        result.Feedback = verdict.Feedback;
        result.Issues = verdict.Issues.Select(issue => new CodeIssueDTO { Line = issue.Line, Message = issue.Message }).ToList();
        result.Status = verdict.Points >= question.Points
            ? QuestionResultStatus.Correct
            : verdict.Points == 0 ? QuestionResultStatus.Incorrect : QuestionResultStatus.PartiallyCorrect;
        return result;
    }

    private static QuestionResultDTO NotChecked(QuestionResultDTO result, string message)
    {
        result.Status = QuestionResultStatus.NotChecked;
        result.Feedback = message;
        return result;
    }

    private static string BuildAiCacheKey(Question question, string answerText)
    {
        var source = string.Join("\u001f",
            question.Id,
            question.Points,
            question.Text,
            question.CodeLanguage ?? string.Empty,
            question.ReferenceAnswer ?? string.Empty,
            question.Rubric ?? string.Empty,
            question.Type == QuestionType.Code ? answerText : Whitespace.Replace(answerText, " "));

        return "ai-review:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private async Task SaveSubmissionAsync(
        Exercise exercise,
        Guid clientId,
        IReadOnlyDictionary<int, QuestionAnswerDTO> answers,
        ExerciseCheckResultDTO result)
    {
        var questions = exercise.Questions.ToDictionary(q => q.Id);
        var submission = new Submission
        {
            ExerciseId = exercise.Id,
            ClientId = clientId,
            Score = result.Score,
            MaxScore = result.MaxScore,
            SubmittedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
        };

        foreach (var questionResult in result.Questions)
        {
            answers.TryGetValue(questionResult.QuestionId, out var answer);
            var type = questions[questionResult.QuestionId].Type;

            submission.Answers.Add(new SubmissionAnswer
            {
                QuestionId = questionResult.QuestionId,
                Status = questionResult.Status,
                EarnedPoints = questionResult.EarnedPoints,
                MaxPoints = questionResult.MaxPoints,
                SelectedOptionIds = (type is QuestionType.SingleChoice or QuestionType.MultipleChoice) && answer is not null
                    ? string.Join(",", answer.SelectedOptionIds.Distinct().Take(10))
                    : null,
                AnswerText = Truncate(PrepareAiAnswer(questions[questionResult.QuestionId], answer?.Text), StoredAnswerMaxLength),
                AiFeedback = questionResult.IsAiReviewed ? Truncate(ComposeAiFeedback(questionResult), 4000) : null,
                AiPoints = questionResult.IsAiReviewed ? questionResult.EarnedPoints : null,
                ReviewStatus = NeedsTeacherReview(type, answer) ? ReviewStatuses.Pending : null
            });
        }

        await _submissionRepository.InsertAsync(submission, true);
    }

    private static bool NeedsTeacherReview(QuestionType type, QuestionAnswerDTO? answer) =>
        (type is QuestionType.OpenAnswer or QuestionType.Code) && !string.IsNullOrWhiteSpace(answer?.Text);

    private static string ComposeAiFeedback(QuestionResultDTO result) =>
        result.Issues.Count == 0
            ? result.Feedback ?? string.Empty
            : $"{result.Feedback}\n{string.Join("\n", result.Issues.Select(issue => $"Рядок {issue.Line}: {issue.Message}"))}";

    private static QuestionStatsDTO BuildQuestionStats(Question question, List<SubmissionAnswer> answers)
    {
        var checkedAnswers = answers.Where(a => a.Status != QuestionResultStatus.NotChecked).ToList();
        var correct = checkedAnswers.Count(a => a.Status == QuestionResultStatus.Correct);
        var partial = checkedAnswers.Count(a => a.Status == QuestionResultStatus.PartiallyCorrect);

        var picks = answers
            .SelectMany(a => ParseIds(a.SelectedOptionIds))
            .GroupBy(optionId => optionId)
            .ToDictionary(g => g.Key, g => g.Count());

        return new QuestionStatsDTO
        {
            QuestionId = question.Id,
            Type = question.Type.ToString(),
            Text = question.Text,
            Answered = checkedAnswers.Count,
            Correct = correct,
            Partial = partial,
            MaxPoints = question.Points,
            AveragePoints = checkedAnswers.Count == 0 ? null : Math.Round(checkedAnswers.Average(a => a.EarnedPoints), 1),
            CorrectPercent = checkedAnswers.Count == 0 ? null : Math.Round(100.0 * correct / checkedAnswers.Count, 1),
            Options = question.Type is QuestionType.SingleChoice or QuestionType.MultipleChoice
                ? question.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new OptionStatsDTO
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                        Picks = picks.GetValueOrDefault(o.Id)
                    })
                    .ToList()
                : new List<OptionStatsDTO>(),
            WrongAnswers = question.Type == QuestionType.ShortAnswer
                ? checkedAnswers
                    .Where(a => a.Status == QuestionResultStatus.Incorrect && !string.IsNullOrWhiteSpace(a.AnswerText))
                    .GroupBy(a => NormalizeShortAnswer(a.AnswerText))
                    .Select(g => new WrongAnswerStatsDTO { Text = g.First().AnswerText!, Count = g.Count() })
                    .OrderByDescending(w => w.Count)
                    .Take(TopWrongAnswers)
                    .ToList()
                : new List<WrongAnswerStatsDTO>()
        };
    }

    private static double Percent(Submission submission) =>
        submission.MaxScore == 0 ? 0 : 100.0 * submission.Score / submission.MaxScore;

    private static double? AveragePercent(IReadOnlyCollection<Submission> submissions) =>
        submissions.Count == 0 ? null : Math.Round(submissions.Average(Percent), 1);

    private static IEnumerable<int> ParseIds(string? value) =>
        (value ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => int.TryParse(part, out var id) ? id : 0)
            .Where(id => id > 0);

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) ? null : value.Length <= maxLength ? value : value[..maxLength];

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
        question.ReferenceAnswer = type switch
        {
            QuestionType.OpenAnswer => NullIfEmpty(dto.ReferenceAnswer),
            QuestionType.Code => NullIfBlankCode(dto.ReferenceAnswer),
            _ => null
        };
        question.Rubric = type is QuestionType.OpenAnswer or QuestionType.Code ? NullIfEmpty(dto.Rubric) : null;
        question.StarterCode = type == QuestionType.Code ? NullIfBlankCode(dto.StarterCode) : null;

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

                case QuestionType.OpenAnswer:
                    if (string.IsNullOrWhiteSpace(question.ReferenceAnswer) && string.IsNullOrWhiteSpace(question.Rubric))
                        throw new ArgumentException($"{label}: вкажіть еталонну відповідь або критерії оцінювання — за ними працює AI-перевірка.");
                    if ((question.ReferenceAnswer?.Length ?? 0) > TeacherFieldMaxLength || (question.Rubric?.Length ?? 0) > TeacherFieldMaxLength)
                        throw new ArgumentException($"{label}: еталон і критерії — не довше {TeacherFieldMaxLength} символів.");
                    break;

                case QuestionType.Code:
                    if (question.CodeLanguage is null || !codeLanguages.Contains(question.CodeLanguage))
                        throw new ArgumentException($"{label}: оберіть мову програмування з мов курсу.");
                    if (string.IsNullOrWhiteSpace(question.ReferenceAnswer) && string.IsNullOrWhiteSpace(question.Rubric))
                        throw new ArgumentException($"{label}: вкажіть еталонне рішення або критерії оцінювання — за ними працює AI-перевірка.");
                    if ((question.ReferenceAnswer?.Length ?? 0) > TeacherFieldMaxLength
                        || (question.Rubric?.Length ?? 0) > TeacherFieldMaxLength
                        || (question.StarterCode?.Length ?? 0) > TeacherFieldMaxLength)
                        throw new ArgumentException($"{label}: еталон, критерії й заготовка — не довше {TeacherFieldMaxLength} символів.");
                    break;
            }
        }
    }

    private static bool HasOptions(QuestionType type) =>
        type is QuestionType.SingleChoice or QuestionType.MultipleChoice or QuestionType.ShortAnswer;

    private static string? NullIfBlankCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Replace("\r\n", "\n").TrimEnd();

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
                StarterCode = q.Type == QuestionType.Code ? q.StarterCode : null,
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
                StarterCode = q.StarterCode,
                Options = q.Options
                    .OrderBy(o => o.Order)
                    .Select(o => new QuestionOptionEditDTO { Id = o.Id, Text = o.Text, IsCorrect = o.IsCorrect })
                    .ToList()
            })
            .ToList()
    };
}
