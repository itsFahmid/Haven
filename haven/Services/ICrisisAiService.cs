using System.Text.Json.Serialization;

namespace Haven.Services;

public class AiCrisisResponse
{
    [JsonPropertyName("message_en")]
    public string MessageEn { get; set; } = string.Empty;

    [JsonPropertyName("message_bn")]
    public string MessageBn { get; set; } = string.Empty;

    [JsonPropertyName("is_off_topic")]
    public bool IsOffTopic { get; set; }

    [JsonPropertyName("is_crisis")]
    public bool IsCrisis { get; set; }

    [JsonPropertyName("suggested_action")]
    public string? SuggestedAction { get; set; }
}

public interface ICrisisAiService
{
    Task<AiCrisisResponse?> AnalyzeMessageAsync(string userMessage, string language);
}
