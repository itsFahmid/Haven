using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("AdminAuditLogs")]
public class AdminAuditLog
{
    [Key]
    public int Id { get; set; }

    public int AdminUserId { get; set; }

    [ForeignKey(nameof(AdminUserId))]
    public User? AdminUser { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TargetResource { get; set; } = string.Empty;

    public string ActionDetails { get; set; } = string.Empty;

    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
