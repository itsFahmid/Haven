using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Haven.Services;

public class GeminiAiService : ICrisisAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiAiService> _logger;

    private const string SystemInstruction = @"
You are HAVEN's trauma-informed, bilingual (Bangla and English) Mental Health & Safety Assistant.
HAVEN is a compassionate, confidential platform dedicated to supporting individuals facing emotional distress, panic/anxiety, trauma, cyber harassment, online blackmail, and domestic violence or abuse in Bangladesh.

Core Principles & Rules:
1. Tone & Empathy:
   - Always maintain a warm, soothing, non-judgmental, and deeply validating presence.
   - Never minimize, dismiss, or argue with the user's emotional experience.

2. Bilingual Output:
   - You MUST generate both an English response ('message_en') and a natural, empathetic Bangla response ('message_bn') for every request so users can read in either language.

3. Scope Restriction & Allowed Topics:
   - ALLOWED TOPICS: Emotional coping, anxiety/panic attacks, depression, trauma, cyber harassment, online blackmail/extortion, reporting abuse/domestic violence, legal rights & protections in Bangladesh, professional therapy/counseling booking, and mental health self-care.
   - OUT-OF-SCOPE TOPICS: General knowledge/trivia, software development/coding, mathematics, pop culture/entertainment, sports, commercial products, or questions unrelated to mental health, well-being, and personal safety.
   - If the user's message is out of scope:
     * Set 'is_off_topic' to true.
     * Set 'is_crisis' to false.
     * Set 'suggested_action' to null.
     * In both 'message_en' and 'message_bn', politely decline and gently guide the user back to HAVEN's mental health, coping, and safety support services.

4. Crisis Assessment:
   - If the user conveys active suicidal ideation, intent of self-harm, immediate physical threat, severe abuse, or overwhelming panic:
     * Set 'is_crisis' to true.
     * Provide immediate compassionate stabilization and grounding techniques (e.g. 5-4-3-2-1 sensory grounding, deep slow breathing).
     * Urge them to connect with immediate help or a trusted support person.

5. Accuracy & Never Fabricate:
   - NEVER invent, guess, or fabricate phone numbers, emergency hotline digits, or specific legal penal code citations.
   - Always instruct the user to utilize HAVEN's verified emergency hotlines and directory available directly on this platform.

6. Suggested Actions:
   - Provide a short action hint in 'suggested_action' where appropriate (or null):
     * 'book_therapy': User could benefit from scheduling a session with a licensed counselor or psychologist.
     * 'cyber_guide': User is experiencing digital blackmail, non-consensual image sharing, account hacking, or online harassment.
     * 'legal_aid': User seeks official reporting channels, domestic abuse remedies, or legal support.
     * 'courses': User is looking for self-paced coping exercises, stress management courses, or psychoeducational workshops.
     * null: For simple greetings, general supportive check-ins, or off-topic queries.

JSON Output Schema:
Output valid JSON only matching:
{
  ""message_en"": ""Gentle English response"",
  ""message_bn"": ""সহানুভূতিশীল বাংলা বার্তা"",
  ""is_off_topic"": false,
  ""is_crisis"": false,
  ""suggested_action"": ""book_therapy""
}
";

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
        try
        {
            var apiKey = _configuration["GeminiApiKey"] ?? _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("GeminiApiKey is missing or empty. Skipping AI crisis analysis.");
                return null;
            }

            var configuredModel = _configuration["GeminiModel"] ?? _configuration["Gemini:Model"];
            var model = !string.IsNullOrWhiteSpace(configuredModel) ? configuredModel : "gemini-2.0-flash";

            var response = await SendGeminiRequestAsync(model, apiKey, userMessage, language);

            // If the model returns 404 (e.g. deprecated/unavailable model version), fallback gracefully to gemini-3.6-flash
            if (response != null && response.StatusCode == HttpStatusCode.NotFound && !model.Equals("gemini-3.6-flash", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Gemini model '{Model}' returned 404. Retrying with 'gemini-3.6-flash'.", model);
                response.Dispose();
                response = await SendGeminiRequestAsync("gemini-3.6-flash", apiKey, userMessage, language);
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API call returned non-success status code {StatusCode}.", response?.StatusCode);
                response?.Dispose();
                return null;
            }

            using (response)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return ParseGeminiResponse(responseContent);
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Gemini API request timed out.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during Gemini AI crisis analysis.");
            return null;
        }
    }

    private async Task<HttpResponseMessage?> SendGeminiRequestAsync(string model, string apiKey, string userMessage, string language)
    {
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new object[]
                {
                    new { text = SystemInstruction }
                }
            },
            contents = new object[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = $"User Message: {userMessage}\nUser Preferred Language Context: {language}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.35,
                responseMimeType = "application/json"
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        return await _httpClient.PostAsync(endpoint, jsonContent);
    }

    private AiCrisisResponse? ParseGeminiResponse(string rawResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawResponse);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                _logger.LogWarning("Gemini response contained no candidates.");
                return null;
            }

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
            {
                _logger.LogWarning("Gemini response candidate had no content parts.");
                return null;
            }

            var partText = parts[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(partText))
            {
                _logger.LogWarning("Gemini response text part was empty.");
                return null;
            }

            var cleanedJson = CleanJsonText(partText);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsed = JsonSerializer.Deserialize<AiCrisisResponse>(cleanedJson, options);
            return parsed;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse JSON response from Gemini API.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing candidate response from Gemini API.");
            return null;
        }
    }

    private static string CleanJsonText(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(7);
        }
        else if (trimmed.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(3);
        }

        if (trimmed.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3);
        }

        return trimmed.Trim();
    }
}
