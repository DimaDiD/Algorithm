namespace MMSA.BLL.Options;

public sealed class AiReviewOptions
{
    public const string SectionName = "AiReview";

    public bool Enabled { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string? WorkspaceId { get; set; }
    public string BaseUrl { get; set; } = "https://api.anthropic.com/";
    public string Model { get; set; } = "claude-haiku-5-5";
    public int MaxOutputTokens { get; set; } = 600;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxAnswerLength { get; set; } = 2000;
    public int DailyLimitPerClient { get; set; } = 50;
    public int DailyLimitTotal { get; set; } = 1000;
    public int CacheHours { get; set; } = 24;
    public decimal InputPricePerMillionTokens { get; set; } = 0.10m;
    public decimal OutputPricePerMillionTokens { get; set; } = 0.50m;
}
