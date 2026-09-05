using Microsoft.AspNetCore.SignalR;
using Haven.Models;
using Haven.Services;

namespace Haven.Hubs;

public class HotlineHub : Hub
{
    private readonly ICrisisAiService _crisisAiService;

    private static readonly HashSet<string> AcuteKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "suicide", "kill myself", "die", "end my life", "hanging", "poison", "self harm", "cut myself", "depressed",
        "আত্মহত্যা", "মরে যাব", "মরতে চাই", "বাঁচতে চাই না", "বেঁচে থাকতে চাই না", "ফাঁস", "বিষ", "নিজেকে শেষ", "হাত কাটা", "কষ্ট সহ্য হচ্ছে না"
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
            message = "হেভেন গোপনীয় আইনি ও মানসিক সুরক্ষা চ্যাটে সংযুক্ত হয়েছেন। আপনার পরিচয় গোপন রাখা হয়েছে।"
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
                messageBn = "আমি বুঝতে পারছি আপনি এই মুহূর্তে তীব্র মানসিক কষ্টের মধ্য দিয়ে যাচ্ছেন। একটি কথা মনে রাখবেন: **আপনি একা নন, আপনার জীবনের মূল্য অপরিসীম।** অনুগ্রহ করে এখনই বিনামূল্যে আমাদের সংকটকালীন কাউন্সেলরদের সাথে যোগাযোগ করুন। তারা ভালোবাসা ও সহমর্মিতা নিয়ে আপনার পাশে আছেন।";
                crisisHelpline = "1098 / 01779554391";
            }
            else if (text.Contains("blackmail") || text.Contains("photo") || text.Contains("ছবি") || text.Contains("ব্ল্যাকমেইল") || text.Contains("হুমকি"))
            {
                messageEn = "🛡️ **Cyber Safety Protocol Initiated:**\n1. Do **NOT** pay any money or send more photos.\n2. **Take full-screen screenshots** with timestamps, URL, and profile IDs.\n3. Do not delete chats—they are legal evidence.\n4. Call **Child Helpline 1098** or contact **Police Cyber Support for Women (01320000888)** or National Emergency **999** immediately.";
                messageBn = "🛡️ **সাইবার ব্ল্যাকমেইল প্রতিরোধ জরুরি গাইড:**\n১. অপরাধীকে কোনো টাকা পাঠাবেন না বা কোনো শর্তে রাজি হবেন না।\n২. অপরাধীর প্রোফাইল লিংক, চ্যাট ও তারিখের স্পষ্ট স্ক্রিনশট সংগ্রহ করুন।\n৩. চ্যাট হিস্ট্রি ডিলিট করবেন না—এটি আইনি প্রমাণ।\n৪. দ্রুত **চাইল্ড হেল্পলাইন ১০৯৮**, **পুলিশ সাইবার সাপোর্ট উইমেন (০১৩২-০০০০৮৮৮)** বা **৯৯৯** এ যোগাযোগ করুন।";
            }
            else if (text.Contains("panic") || text.Contains("anxiety") || text.Contains("ভয়") || text.Contains("প্যানিক") || text.Contains("অস্থির"))
            {
                messageEn = "🌿 **Let's Pause Together:** You are safe in this moment. Try the **4-7-8 Breathing Technique**:\n- Inhale slowly through your nose for **4 seconds**\n- Hold your breath gently for **7 seconds**\n- Exhale slowly through your mouth for **8 seconds**.\nNotice 5 things you can see around you right now.";
                messageBn = "🌿 **চলুন একসাথে একটি দীর্ঘ শ্বাস নেই:** এই মুহূর্তে আপনি নিরাপদ আছেন। **৪-৭-৮ ব্রিদিং পদ্ধতি** চেষ্টা করুন:\n- নাক দিয়ে ৪ সেকেন্ড ধীরে ধীরে শ্বাস নিন\n- ৭ সেকেন্ড শ্বাসটি ধরে রাখুন\n- মুখ দিয়ে ৮ সেকেন্ড ধরে ধীরে ধীরে শ্বাস ছাড়ুন।\nআপনার চারপাশের ৫টি শান্ত বস্তু লক্ষ্য করুন।";
            }
            else
            {
                messageEn = "Thank you for reaching out. HAVEN is your completely anonymous safe sanctuary. How can I best support you right now? You can ask about cyber safety, emotional coping, reporting abuse, or booking a confidential therapist.";
                messageBn = "আমাদের কাছে লেখার জন্য ধন্যবাদ। হেভেন আপনার ১০০% নিরাপদ ও বেনামী আশ্রয়স্থল। আমি আপনাকে কীভাবে সহায়তা করতে পারি? সাইবার নিরাপত্তা, মানসিক স্বাস্থ্য, নির্যাতন প্রতিকার বা থেরাপিস্ট বুকিং সম্পর্কে যেকোনো প্রশ্ন করতে পারেন।";
            }
        }

        if (triggerEscalationModal || isHighRisk)
        {
            await Clients.Caller.SendAsync("AcuteDangerAlert", new
            {
                alert = "জরুরি সংকট সনাক্ত করা হয়েছে। অনুগ্রহ করে ১০৯৮ বা ৯৯৯ এ কল করুন।",
                escalate = true,
                hotlines = new[] {
                    new { name = "জাতীয় জরুরি সেবা", number = "999", type = "Emergency" },
                    new { name = "চাইল্ড হেল্পলাইন", number = "1098", type = "ChildProtection" },
                    new { name = "কান পেতে রই", number = "01779554391", type = "Emotional" }
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
