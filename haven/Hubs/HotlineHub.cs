using Microsoft.AspNetCore.SignalR;
using Haven.Models;
using Haven.Services;

namespace Haven.Hubs;

public class HotlineHub : Hub
{
    private static readonly HashSet<string> AcuteKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "suicide", "self-harm", "self harm", "kill myself", "want to die",
        "আত্মহত্যা", "নিজের ক্ষতি", "মরে যেতে চাই", "বাঁচতে চাই না", "অত্যাচার", "রেপ", "rape"
    };

    public HotlineHub()
    {
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

    public async Task SendMessage(string senderAlias, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText)) return;

        string connectionId = Context.ConnectionId;
        string roomName = $"Session_{connectionId}";

        // Triage check for acute distress keywords
        bool isAcuteDanger = AcuteKeywords.Any(k => messageText.Contains(k, StringComparison.OrdinalIgnoreCase));

        if (isAcuteDanger)
        {
            // Surface crisis alert to user
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

        // Echo message back to session group
        await Clients.Group(roomName).SendAsync("ReceiveMessage", new
        {
            sender = string.IsNullOrWhiteSpace(senderAlias) ? "Anonymous Youth" : senderAlias,
            message = messageText,
            timestamp = DateTime.UtcNow.ToString("hh:mm tt"),
            isAcuteDanger
        });
    }
}
