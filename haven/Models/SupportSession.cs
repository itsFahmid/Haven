using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("SupportSessions")]
public class SupportSession
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int? ResponderId { get; set; }

    [ForeignKey(nameof(ResponderId))]
    public User? Responder { get; set; }

    [MaxLength(50)]
    public string SessionType { get; set; } = "AnonymousHotline"; // AI, LiveOperator, AnonymousHotline

    public bool IsEscalated { get; set; } = false;

    [MaxLength(100)]
    public string? TriggerKeyword { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? EndedAt { get; set; }
}
