using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("CrisisAlerts")]
public class CrisisAlert
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string TriggerKeyword { get; set; } = string.Empty;

    [MaxLength(50)]
    public string SeverityLevel { get; set; } = "High"; // Low, Medium, High, Critical

    public string? ActionTaken { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
