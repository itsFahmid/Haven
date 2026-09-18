using Microsoft.AspNetCore.SignalR;
using Obhoy.Models;
using Obhoy.Services;

namespace Obhoy.Hubs;

public class HotlineHub : Hub
{
    private readonly ICrisisAiService _crisisAiService;

    private static readonly HashSet<string> AcuteKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "suicide", "kill myself", "die", "end my life", "hanging", "poison", "self harm", "cut myself", "depressed",
        "à¦†à¦¤à§à¦®à¦¹à¦¤à§à¦¯à¦¾", "à¦®à¦°à§‡ à¦¯à¦¾à¦¬", "à¦®à¦°à¦¤à§‡ à¦šà¦¾à¦‡", "à¦¬à¦¾à¦à¦šà¦¤à§‡ à¦šà¦¾à¦‡ à¦¨à¦¾", "à¦¬à§‡à¦à¦šà§‡ à¦¥à¦¾à¦•à¦¤à§‡ à¦šà¦¾à¦‡ à¦¨à¦¾", "à¦«à¦¾à¦à¦¸", "à¦¬à¦¿à¦·", "à¦¨à¦¿à¦œà§‡à¦•à§‡ à¦¶à§‡à¦·", "à¦¹à¦¾à¦¤ à¦•à¦¾à¦Ÿà¦¾", "à¦•à¦·à§à¦Ÿ à¦¸à¦¹à§à¦¯ à¦¹à¦šà§à¦›à§‡ à¦¨à¦¾"
    };

    public HotlineHub(ICrisisAiService crisisAiService)
    {
        _crisisAiService = crisisAiService;
    }

    public async Task JoinHotlineSession(string sessionType = "AnonymousHotline")
    {
        string connectionId = Context.ConnectionId;
        string roomName = $"Session_{connectionId}";
        await Groups.AddToGroupAsync(connectionId, roomName);

        await Clients.Caller.SendAsync("SessionInitialized", new
        {
            connectionId,
            roomName,
            status = "Connected",
            message = "à¦¹à§‡à¦­à§‡à¦¨ à¦—à§‹à¦ªà¦¨à§€à§Ÿ à¦†à¦‡à¦¨à¦¿ à¦“ à¦®à¦¾à¦¨à¦¸à¦¿à¦• à¦¸à§à¦°à¦•à§à¦·à¦¾ à¦šà§à¦¯à¦¾à¦Ÿà§‡ à¦¸à¦‚à¦¯à§à¦•à§à¦¤ à¦¹à§Ÿà§‡à¦›à§‡à¦¨à¥¤ à¦†à¦ªà¦¨à¦¾à¦° à¦ªà¦°à¦¿à¦šà§Ÿ à¦—à§‹à¦ªà¦¨ à¦°à¦¾à¦–à¦¾ à¦¹à§Ÿà§‡à¦›à§‡à¥¤"
        });
    }

    public async Task SendMessage(string senderAlias, string messageText, string lang = "bn")
    {
        if (string.IsNullOrWhiteSpace(messageText)) return;

        string connectionId = Context.ConnectionId;
        string roomName = $"Session_{connectionId}";
        var text = messageText.Trim();

        // 1. Primary: Gemini AI Analysis
        var aiResult = await _crisisAiService.AnalyzeMessageAsync(text, lang);

        bool isHighRisk = false;
        string messageEn;
        string messageBn;
        string? crisisHelpline = null;
        bool triggerEscalationModal = false;

        if (aiResult != null)
        {
            isHighRisk = aiResult.IsCrisis;
            messageEn = aiResult.MessageEn;
            messageBn = aiResult.MessageBn;
            triggerEscalationModal = aiResult.IsCrisis;
            crisisHelpline = aiResult.IsCrisis ? "1098 / 01779554391" : null;
        }
        else
        {
            // 2. Safety Net Fallback: Keyword-based crisis detection (Bengali & English)
            isHighRisk = AcuteKeywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
            triggerEscalationModal = isHighRisk;

            if (isHighRisk)
            {
                messageEn = "I can feel how much pain you are holding right now, and I want you to know: **You are not alone, and your life matters deeply.** Please reach out right now to a certified crisis counselor who cares and is waiting to support you unconditionally.";
                messageBn = "à¦†à¦®à¦¿ à¦¬à§à¦à¦¤à§‡ à¦ªà¦¾à¦°à¦›à¦¿ à¦†à¦ªà¦¨à¦¿ à¦à¦‡ à¦®à§à¦¹à§‚à¦°à§à¦¤à§‡ à¦¤à§€à¦¬à§à¦° à¦®à¦¾à¦¨à¦¸à¦¿à¦• à¦•à¦·à§à¦Ÿà§‡à¦° à¦®à¦§à§à¦¯ à¦¦à¦¿à¦¯à¦¼à§‡ à¦¯à¦¾à¦šà§à¦›à§‡à¦¨à¥¤ à¦à¦•à¦Ÿà¦¿ à¦•à¦¥à¦¾ à¦®à¦¨à§‡ à¦°à¦¾à¦–à¦¬à§‡à¦¨: **à¦†à¦ªà¦¨à¦¿ à¦à¦•à¦¾ à¦¨à¦¨, à¦†à¦ªà¦¨à¦¾à¦° à¦œà§€à¦¬à¦¨à§‡à¦° à¦®à§‚à¦²à§à¦¯ à¦…à¦ªà¦°à¦¿à¦¸à§€à¦®à¥¤** à¦…à¦¨à§à¦—à§à¦°à¦¹ à¦•à¦°à§‡ à¦à¦–à¦¨à¦‡ à¦¬à¦¿à¦¨à¦¾à¦®à§‚à¦²à§à¦¯à§‡ à¦†à¦®à¦¾à¦¦à§‡à¦° à¦¸à¦‚à¦•à¦Ÿà¦•à¦¾à¦²à§€à¦¨ à¦•à¦¾à¦‰à¦¨à§à¦¸à§‡à¦²à¦°à¦¦à§‡à¦° à¦¸à¦¾à¦¥à§‡ à¦¯à§‹à¦—à¦¾à¦¯à§‹à¦— à¦•à¦°à§à¦¨à¥¤ à¦¤à¦¾à¦°à¦¾ à¦­à¦¾à¦²à§‹à¦¬à¦¾à¦¸à¦¾ à¦“ à¦¸à¦¹à¦®à¦°à§à¦®à¦¿à¦¤à¦¾ à¦¨à¦¿à§Ÿà§‡ à¦†à¦ªà¦¨à¦¾à¦° à¦ªà¦¾à¦¶à§‡ à¦†à¦›à§‡à¦¨à¥¤";
                crisisHelpline = "1098 / 01779554391";
            }
            else if (text.Contains("blackmail") || text.Contains("photo") || text.Contains("à¦›à¦¬à¦¿") || text.Contains("à¦¬à§à¦²à§à¦¯à¦¾à¦•à¦®à§‡à¦‡à¦²") || text.Contains("à¦¹à§à¦®à¦•à¦¿"))
            {
                messageEn = "ðŸ›¡ï¸ **Cyber Safety Protocol Initiated:**\n1. Do **NOT** pay any money or send more photos.\n2. **Take full-screen screenshots** with timestamps, URL, and profile IDs.\n3. Do not delete chatsâ€”they are legal evidence.\n4. Call **Child Helpline 1098** or contact **Police Cyber Support for Women (01320000888)** or National Emergency **999** immediately.";
                messageBn = "ðŸ›¡ï¸ **à¦¸à¦¾à¦‡à¦¬à¦¾à¦° à¦¬à§à¦²à§à¦¯à¦¾à¦•à¦®à§‡à¦‡à¦² à¦ªà§à¦°à¦¤à¦¿à¦°à§‹à¦§ à¦œà¦°à§à¦°à¦¿ à¦—à¦¾à¦‡à¦¡:**\nà§§. à¦…à¦ªà¦°à¦¾à¦§à§€à¦•à§‡ à¦•à§‹à¦¨à§‹ à¦Ÿà¦¾à¦•à¦¾ à¦ªà¦¾à¦ à¦¾à¦¬à§‡à¦¨ à¦¨à¦¾ à¦¬à¦¾ à¦•à§‹à¦¨à§‹ à¦¶à¦°à§à¦¤à§‡ à¦°à¦¾à¦œà¦¿ à¦¹à¦¬à§‡à¦¨ à¦¨à¦¾à¥¤\nà§¨. à¦…à¦ªà¦°à¦¾à¦§à§€à¦° à¦ªà§à¦°à§‹à¦«à¦¾à¦‡à¦² à¦²à¦¿à¦‚à¦•, à¦šà§à¦¯à¦¾à¦Ÿ à¦“ à¦¤à¦¾à¦°à¦¿à¦–à§‡à¦° à¦¸à§à¦ªà¦·à§à¦Ÿ à¦¸à§à¦•à§à¦°à¦¿à¦¨à¦¶à¦Ÿ à¦¸à¦‚à¦—à§à¦°à¦¹ à¦•à¦°à§à¦¨à¥¤\nà§©. à¦šà§à¦¯à¦¾à¦Ÿ à¦¹à¦¿à¦¸à§à¦Ÿà§à¦°à¦¿ à¦¡à¦¿à¦²à¦¿à¦Ÿ à¦•à¦°à¦¬à§‡à¦¨ à¦¨à¦¾â€”à¦à¦Ÿà¦¿ à¦†à¦‡à¦¨à¦¿ à¦ªà§à¦°à¦®à¦¾à¦£à¥¤\nà§ª. à¦¦à§à¦°à§à¦¤ **à¦šà¦¾à¦‡à¦²à§à¦¡ à¦¹à§‡à¦²à§à¦ªà¦²à¦¾à¦‡à¦¨ à§§à§¦à§¯à§®**, **à¦ªà§à¦²à¦¿à¦¶ à¦¸à¦¾à¦‡à¦¬à¦¾à¦° à¦¸à¦¾à¦ªà§‹à¦°à§à¦Ÿ à¦‰à¦‡à¦®à§‡à¦¨ (à§¦à§§à§©à§¨-à§¦à§¦à§¦à§¦à§®à§®à§®)** à¦¬à¦¾ **à§¯à§¯à§¯** à¦ à¦¯à§‹à¦—à¦¾à¦¯à§‹à¦— à¦•à¦°à§à¦¨à¥¤";
            }
            else if (text.Contains("panic") || text.Contains("anxiety") || text.Contains("à¦­à¦¯à¦¼") || text.Contains("à¦ªà§à¦¯à¦¾à¦¨à¦¿à¦•") || text.Contains("à¦…à¦¸à§à¦¥à¦¿à¦°"))
            {
                messageEn = "ðŸŒ¿ **Let's Pause Together:** You are safe in this moment. Try the **4-7-8 Breathing Technique**:\n- Inhale slowly through your nose for **4 seconds**\n- Hold your breath gently for **7 seconds**\n- Exhale slowly through your mouth for **8 seconds**.\nNotice 5 things you can see around you right now.";
                messageBn = "ðŸŒ¿ **à¦šà¦²à§à¦¨ à¦à¦•à¦¸à¦¾à¦¥à§‡ à¦à¦•à¦Ÿà¦¿ à¦¦à§€à¦°à§à¦˜ à¦¶à§à¦¬à¦¾à¦¸ à¦¨à§‡à¦‡:** à¦à¦‡ à¦®à§à¦¹à§‚à¦°à§à¦¤à§‡ à¦†à¦ªà¦¨à¦¿ à¦¨à¦¿à¦°à¦¾à¦ªà¦¦ à¦†à¦›à§‡à¦¨à¥¤ **à§ª-à§­-à§® à¦¬à§à¦°à¦¿à¦¦à¦¿à¦‚ à¦ªà¦¦à§à¦§à¦¤à¦¿** à¦šà§‡à¦·à§à¦Ÿà¦¾ à¦•à¦°à§à¦¨:\n- à¦¨à¦¾à¦• à¦¦à¦¿à¦¯à¦¼à§‡ à§ª à¦¸à§‡à¦•à§‡à¦¨à§à¦¡ à¦§à§€à¦°à§‡ à¦§à§€à¦°à§‡ à¦¶à§à¦¬à¦¾à¦¸ à¦¨à¦¿à¦¨\n- à§­ à¦¸à§‡à¦•à§‡à¦¨à§à¦¡ à¦¶à§à¦¬à¦¾à¦¸à¦Ÿà¦¿ à¦§à¦°à§‡ à¦°à¦¾à¦–à§à¦¨\n- à¦®à§à¦– à¦¦à¦¿à¦¯à¦¼à§‡ à§® à¦¸à§‡à¦•à§‡à¦¨à§à¦¡ à¦§à¦°à§‡ à¦§à§€à¦°à§‡ à¦§à§€à¦°à§‡ à¦¶à§à¦¬à¦¾à¦¸ à¦›à¦¾à¦¡à¦¼à§à¦¨à¥¤\nà¦†à¦ªà¦¨à¦¾à¦° à¦šà¦¾à¦°à¦ªà¦¾à¦¶à§‡à¦° à§«à¦Ÿà¦¿ à¦¶à¦¾à¦¨à§à¦¤ à¦¬à¦¸à§à¦¤à§ à¦²à¦•à§à¦·à§à¦¯ à¦•à¦°à§à¦¨à¥¤";
            }
            else
            {
                messageEn = "Thank you for reaching out. Obhoy is your completely anonymous safe sanctuary. How can I best support you right now? You can ask about cyber safety, emotional coping, reporting abuse, or booking a confidential therapist.";
                messageBn = "à¦†à¦®à¦¾à¦¦à§‡à¦° à¦•à¦¾à¦›à§‡ à¦²à§‡à¦–à¦¾à¦° à¦œà¦¨à§à¦¯ à¦§à¦¨à§à¦¯à¦¬à¦¾à¦¦à¥¤ à¦¹à§‡à¦­à§‡à¦¨ à¦†à¦ªà¦¨à¦¾à¦° à§§à§¦à§¦% à¦¨à¦¿à¦°à¦¾à¦ªà¦¦ à¦“ à¦¬à§‡à¦¨à¦¾à¦®à§€ à¦†à¦¶à§à¦°à¦¯à¦¼à¦¸à§à¦¥à¦²à¥¤ à¦†à¦®à¦¿ à¦†à¦ªà¦¨à¦¾à¦•à§‡ à¦•à§€à¦­à¦¾à¦¬à§‡ à¦¸à¦¹à¦¾à¦¯à¦¼à¦¤à¦¾ à¦•à¦°à¦¤à§‡ à¦ªà¦¾à¦°à¦¿? à¦¸à¦¾à¦‡à¦¬à¦¾à¦° à¦¨à¦¿à¦°à¦¾à¦ªà¦¤à§à¦¤à¦¾, à¦®à¦¾à¦¨à¦¸à¦¿à¦• à¦¸à§à¦¬à¦¾à¦¸à§à¦¥à§à¦¯, à¦¨à¦¿à¦°à§à¦¯à¦¾à¦¤à¦¨ à¦ªà§à¦°à¦¤à¦¿à¦•à¦¾à¦° à¦¬à¦¾ à¦¥à§‡à¦°à¦¾à¦ªà¦¿à¦¸à§à¦Ÿ à¦¬à§à¦•à¦¿à¦‚ à¦¸à¦®à§à¦ªà¦°à§à¦•à§‡ à¦¯à§‡à¦•à§‹à¦¨à§‹ à¦ªà§à¦°à¦¶à§à¦¨ à¦•à¦°à¦¤à§‡ à¦ªà¦¾à¦°à§‡à¦¨à¥¤";
            }
        }

        if (triggerEscalationModal || isHighRisk)
        {
            await Clients.Caller.SendAsync("AcuteDangerAlert", new
            {
                alert = "à¦œà¦°à§à¦°à¦¿ à¦¸à¦‚à¦•à¦Ÿ à¦¸à¦¨à¦¾à¦•à§à¦¤ à¦•à¦°à¦¾ à¦¹à§Ÿà§‡à¦›à§‡à¥¤ à¦…à¦¨à§à¦—à§à¦°à¦¹ à¦•à¦°à§‡ à§§à§¦à§¯à§® à¦¬à¦¾ à§¯à§¯à§¯ à¦ à¦•à¦² à¦•à¦°à§à¦¨à¥¤",
                escalate = true,
                hotlines = new[] {
                    new { name = "à¦œà¦¾à¦¤à§€à¦¯à¦¼ à¦œà¦°à§à¦°à¦¿ à¦¸à§‡à¦¬à¦¾", number = "999", type = "Emergency" },
                    new { name = "à¦šà¦¾à¦‡à¦²à§à¦¡ à¦¹à§‡à¦²à§à¦ªà¦²à¦¾à¦‡à¦¨", number = "1098", type = "ChildProtection" },
                    new { name = "à¦•à¦¾à¦¨ à¦ªà§‡à¦¤à§‡ à¦°à¦‡", number = "01779554391", type = "Emotional" }
                }
            });
        }

        // Echo user message to session group
        await Clients.Group(roomName).SendAsync("ReceiveMessage", new
        {
            sender = string.IsNullOrWhiteSpace(senderAlias) ? "Anonymous Ally" : senderAlias,
            message = text,
            timestamp = DateTime.UtcNow.ToString("hh:mm tt"),
            isAcuteDanger = isHighRisk
        });

        // Send bot triage response back to caller
        await Clients.Caller.SendAsync("ReceiveBotResponse", new
        {
            messageEn,
            messageBn,
            isHighRisk,
            triggerEscalationModal,
            crisisHelpline,
            timestamp = DateTime.UtcNow.ToString("hh:mm tt")
        });
    }
}
