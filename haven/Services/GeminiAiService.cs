using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Haven.Services;

public class GeminiAiService : ICrisisAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AiCrisisResponse?> AnalyzeMessageAsync(string userMessage, string language)
    {
        var apiKey = _configuration["GeminiApiKey"] ?? _configuration["Gemini:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("GeminiApiKey is missing or empty. Skipping AI crisis analysis.");
            return null;
        }

        // Placeholder for Gemini API HTTP request (to be implemented in Phase 2)
        await Task.CompletedTask;
        return null;
    }
}
