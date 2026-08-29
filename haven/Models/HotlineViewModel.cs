namespace Haven.Models;

public class HotlineViewModel
{
    public string AnonymousSessionId { get; set; } = "HAVEN-ANON-" + Random.Shared.Next(1000, 9999);
    public string UserAlias { get; set; } = "Anonymous Ally #" + Random.Shared.Next(100, 999);
    public string UserAliasBn { get; set; } = "বেনামী বন্ধু #" + Random.Shared.Next(100, 999);
    public List<EmergencyContact> EmergencyHotlines { get; set; } = new();
    public List<QuickHelpPrompt> QuickPrompts { get; set; } = new();
}

public class QuickHelpPrompt
{
    public string PromptEn { get; set; } = string.Empty;
    public string PromptBn { get; set; } = string.Empty;
    public string CategoryEn { get; set; } = string.Empty;
    public string CategoryBn { get; set; } = string.Empty;
    public string Icon { get; set; } = "chat";
}

public class ChatMessageItem
{
    public string Sender { get; set; } = "bot"; // "user", "bot", "system", "counselor"
    public string MessageEn { get; set; } = string.Empty;
    public string MessageBn { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsHighRisk { get; set; }
    public string? SuggestedActionEn { get; set; }
    public string? SuggestedActionBn { get; set; }
}
