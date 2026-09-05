namespace Haven.Services;

public class AiCrisisResponse
{
    public string MessageEn { get; set; } = string.Empty;
    public string MessageBn { get; set; } = string.Empty;
    public bool IsOffTopic { get; set; }
    public bool IsCrisis { get; set; }
    public string SuggestedAction { get; set; } = string.Empty;
}

public interface ICrisisAiService
{
    Task<AiCrisisResponse?> AnalyzeMessageAsync(string userMessage, string language);
}
