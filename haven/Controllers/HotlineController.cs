using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Haven.Models;
using Haven.Services;

namespace Haven.Controllers;

public class HotlineController : Controller
{
    private readonly ICrisisAiService _crisisAiService;
    private readonly IConfiguration _configuration;

    public HotlineController(ICrisisAiService crisisAiService, IConfiguration configuration)
    {
        _crisisAiService = crisisAiService;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GeminiStatus()
    {
        static string? Resolve(params string?[] candidates) =>
            candidates.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));

        var apiKey = Resolve(
            _configuration["GeminiApiKey"],
            _configuration["Gemini:ApiKey"],
            _configuration["GEMINI_API_KEY"],
            _configuration["Gemini__ApiKey"],
            _configuration["Gemini_API"],
            _configuration["GEMINI_API"],
            Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
            Environment.GetEnvironmentVariable("GeminiApiKey"),
            Environment.GetEnvironmentVariable("Gemini_API"),
            Environment.GetEnvironmentVariable("GEMINI_API")
        );

        var model = Resolve(
            _configuration["GeminiModel"],
            _configuration["Gemini:Model"],
            _configuration["GEMINI_MODEL"],
            Environment.GetEnvironmentVariable("GEMINI_MODEL")
        ) ?? "gemini-3.6-flash";

        string? liveTestStatus = null;
        string? liveTestBody = null;

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            try
            {
                using var http = new System.Net.Http.HttpClient();
                http.Timeout = TimeSpan.FromSeconds(15);
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                var payload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    contents = new[] { new { parts = new[] { new { text = "hi" } } } }
                });
                var resp = await http.PostAsync(endpoint, new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
                liveTestStatus = ((int)resp.StatusCode).ToString() + " " + resp.StatusCode.ToString();
                var body = await resp.Content.ReadAsStringAsync();
                liveTestBody = body.Length > 500 ? body[..500] + "..." : body;
            }
            catch (Exception ex)
            {
                liveTestStatus = "Exception";
                liveTestBody = ex.Message;
            }
        }

        return Json(new
        {
            keyFound = !string.IsNullOrWhiteSpace(apiKey),
            keyPrefix = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Substring(0, Math.Min(8, apiKey.Length)) + "...",
            keyLength = apiKey?.Length ?? 0,
            model,
            liveTestStatus,
            liveTestBody,
            allEnvKeys = System.Environment.GetEnvironmentVariables().Keys
                .Cast<string>()
                .Where(k => k.Contains("GEMINI", StringComparison.OrdinalIgnoreCase) || k.Contains("Gemini", StringComparison.OrdinalIgnoreCase))
                .ToList()
        });
    }

    public IActionResult Index()
    {
        var model = new HotlineViewModel
        {
            AnonymousSessionId = "HAVEN-ANON-" + Random.Shared.Next(1000, 9999),
            UserAlias = "Anonymous Ally #" + Random.Shared.Next(100, 999),
            UserAliasBn = "বেনামী বন্ধু #" + Random.Shared.Next(100, 999),
            EmergencyHotlines = HavenDataStore.GetEmergencyHotlines(),
            QuickPrompts = HavenDataStore.GetQuickPrompts()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Text))
        {
            return BadRequest(new { error = "Message cannot be empty." });
        }

        var text = request.Text.Trim();

        // 1. Primary: Gemini AI Analysis
        var aiResult = await _crisisAiService.AnalyzeMessageAsync(text, request.Lang);
        if (aiResult != null)
        {
            var aiResponse = new ChatResponse
            {
                IsHighRisk = aiResult.IsCrisis,
                MessageEn = aiResult.MessageEn,
                MessageBn = aiResult.MessageBn,
                TriggerEscalationModal = aiResult.IsCrisis,
                CrisisHelpline = aiResult.IsCrisis ? "1098 / 01779554391" : null,
                Timestamp = DateTime.UtcNow.ToString("hh:mm tt")
            };

            return Json(aiResponse);
        }

        // 2. Safety Net Fallback: Crisis keywords trigger detection (Bengali & English)
        var highRiskKeywords = new[]
        {
            "suicide", "kill myself", "die", "end my life", "hanging", "poison", "self harm", "cut myself", "depressed",
            "আত্মহত্যা", "মরে যাব", "মরতে চাই", "বাঁচতে চাই না", "বেঁচে থাকতে চাই না", "ফাঁস", "বিষ", "নিজেকে শেষ", "হাত কাটা", "কষ্ট সহ্য হচ্ছে না"
        };

        bool isHighRisk = highRiskKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        var response = new ChatResponse
        {
            IsHighRisk = isHighRisk,
            Timestamp = DateTime.UtcNow.ToString("hh:mm tt")
        };

        if (isHighRisk)
        {
            response.MessageEn = "I can feel how much pain you are holding right now, and I want you to know: **You are not alone, and your life matters deeply.** Please reach out right now to a certified crisis counselor who cares and is waiting to support you unconditionally.";
            response.MessageBn = "আমি বুঝতে পারছি আপনি এই মুহূর্তে তীব্র মানসিক কষ্টের মধ্য দিয়ে যাচ্ছেন। একটি কথা মনে রাখবেন: **আপনি একা নন, আপনার জীবনের মূল্য অপরিসীম।** অনুগ্রহ করে এখনই বিনামূল্যে আমাদের সংকটকালীন কাউন্সেলরদের সাথে যোগাযোগ করুন। তারা ভালোবাসা ও সহমর্মিতা নিয়ে আপনার পাশে আছেন।";
            response.CrisisHelpline = "1098 / 01779554391";
            response.TriggerEscalationModal = true;
        }
        else if (text.Contains("blackmail") || text.Contains("photo") || text.Contains("ছবি") || text.Contains("ব্ল্যাকমেইল") || text.Contains("হুমকি"))
        {
            response.MessageEn = "🛡️ **Cyber Safety Protocol Initiated:**\n1. Do **NOT** pay any money or send more photos.\n2. **Take full-screen screenshots** with timestamps, URL, and profile IDs.\n3. Do not delete chats—they are legal evidence.\n4. Call **Child Helpline 1098** or contact **Police Cyber Support for Women (01320000888)** or National Emergency **999** immediately.";
            response.MessageBn = "🛡️ **সাইবার ব্ল্যাকমেইল প্রতিরোধ জরুরি গাইড:**\n১. অপরাধীকে কোনো টাকা পাঠাবেন না বা কোনো শর্তে রাজি হবেন না।\n২. অপরাধীর প্রোফাইল লিংক, চ্যাট ও তারিখের স্পষ্ট স্ক্রিনশট সংগ্রহ করুন।\n৩. চ্যাট হিস্ট্রি ডিলিট করবেন না—এটি আইনি প্রমাণ।\n৪. দ্রুত **চাইল্ড হেল্পলাইন ১০৯৮**, **পুলিশ সাইবার সাপোর্ট উইমেন (০১৩২-০০০০৮৮৮)** বা **৯৯৯** এ যোগাযোগ করুন।";
        }
        else if (text.Contains("panic") || text.Contains("anxiety") || text.Contains("ভয়") || text.Contains("প্যানিক") || text.Contains("অস্থির"))
        {
            response.MessageEn = "🌿 **Let's Pause Together:** You are safe in this moment. Try the **4-7-8 Breathing Technique**:\n- Inhale slowly through your nose for **4 seconds**\n- Hold your breath gently for **7 seconds**\n- Exhale slowly through your mouth for **8 seconds**.\nNotice 5 things you can see around you right now.";
            response.MessageBn = "🌿 **চলুন একসাথে একটি দীর্ঘ শ্বাস নেই:** এই মুহূর্তে আপনি নিরাপদ আছেন। **৪-৭-৮ ব্রিদিং পদ্ধতি** চেষ্টা করুন:\n- নাক দিয়ে ৪ সেকেন্ড ধীরে ধীরে শ্বাস নিন\n- ৭ সেকেন্ড শ্বাসটি ধরে রাখুন\n- মুখ দিয়ে ৮ সেকেন্ড ধরে ধীরে ধীরে শ্বাস ছাড়ুন।\nআপনার চারপাশের ৫টি শান্ত বস্তু লক্ষ্য করুন।";
        }
        else
        {
            response.MessageEn = "Thank you for reaching out. HAVEN is your completely anonymous safe sanctuary. How can I best support you right now? You can ask about cyber safety, emotional coping, reporting abuse, or booking a confidential therapist.";
            response.MessageBn = "আমাদের কাছে লেখার জন্য ধন্যবাদ। হেভেন আপনার ১০০% নিরাপদ ও বেনামী আশ্রয়স্থল। আমি আপনাকে কীভাবে সহায়তা করতে পারি? সাইবার নিরাপত্তা, মানসিক স্বাস্থ্য, নির্যাতন প্রতিকার বা থেরাপিস্ট বুকিং সম্পর্কে যেকোনো প্রশ্ন করতে পারেন।";
        }

        return Json(response);
    }
}

public class ChatMessageRequest
{
    public string Text { get; set; } = string.Empty;
    public string Lang { get; set; } = "bn";
}

public class ChatResponse
{
    public string MessageEn { get; set; } = string.Empty;
    public string MessageBn { get; set; } = string.Empty;
    public bool IsHighRisk { get; set; }
    public string? CrisisHelpline { get; set; }
    public bool TriggerEscalationModal { get; set; }
    public string Timestamp { get; set; } = string.Empty;
}
