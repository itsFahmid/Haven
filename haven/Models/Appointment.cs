using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Appointments")]
public class Appointment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public int ProfessionalId { get; set; }

    [ForeignKey(nameof(ProfessionalId))]
    public ProfessionalProfile? Professional { get; set; }

    public DateTime ScheduledDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string TimeSlot { get; set; } = string.Empty; // e.g. "04:30 PM", "09:00 PM"

    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Rescheduled, Cancelled

    [MaxLength(50)]
    public string CommunicationChannel { get; set; } = "Encrypted Audio"; // Audio, Video, Chat

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
