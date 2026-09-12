using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

public enum BookingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    Completed = 4
}

[Table("Bookings")]
public class Booking
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; } // Patient / User requesting the appointment

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public int TherapistId { get; set; } // ProfessionalProfile Id

    [ForeignKey(nameof(TherapistId))]
    public ProfessionalProfile? Therapist { get; set; }

    [Required(ErrorMessage = "Please select an appointment date.")]
    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "Please select a time slot.")]
    [MaxLength(50)]
    public string TimeSlot { get; set; } = string.Empty; // e.g. "04:30 PM - 05:30 PM"

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string CommunicationMode { get; set; } = "Online Video"; // Online Video, Confidential Audio, In-Person

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [MaxLength(50)]
    public string BookingReference { get; set; } = string.Empty; // e.g. "HVN-BK-84920"

    [Column(TypeName = "decimal(18,2)")]
    public decimal FeeBDT { get; set; } = 0;

    public bool IsFeeSubsidized { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
