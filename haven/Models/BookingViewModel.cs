using System.ComponentModel.DataAnnotations;

namespace Obhoy.Models;

public class BookingCreateModel
{
    [Required]
    public int TherapistId { get; set; }

    [Required(ErrorMessage = "à¦…à¦¨à§à¦—à§à¦°à¦¹ à¦•à¦°à§‡ à¦à¦•à¦Ÿà¦¿ à¦¸à§‡à¦¶à¦¨ à¦¤à¦¾à¦°à¦¿à¦– à¦¨à¦¿à¦°à§à¦¬à¦¾à¦šà¦¨ à¦•à¦°à§à¦¨ / Please select an appointment date.")]
    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "à¦…à¦¨à§à¦—à§à¦°à¦¹ à¦•à¦°à§‡ à¦à¦•à¦Ÿà¦¿ à¦¸à¦®à¦¯à¦¼ à¦¸à§à¦²à¦Ÿ à¦¨à¦¿à¦°à§à¦¬à¦¾à¦šà¦¨ à¦•à¦°à§à¦¨ / Please select a time slot.")]
    [MaxLength(50)]
    public string TimeSlot { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(50)]
    public string CommunicationMode { get; set; } = "Online Video"; // Online Video, Confidential Audio, In-Person

    public bool RequestFeeSubsidy { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

public class PatientBookingsViewModel
{
    public List<Booking> Bookings { get; set; } = new();
    public int TotalBookings => Bookings.Count;
    public int PendingCount => Bookings.Count(b => b.Status == BookingStatus.Pending);
    public int ApprovedCount => Bookings.Count(b => b.Status == BookingStatus.Approved);
    public int RejectedCount => Bookings.Count(b => b.Status == BookingStatus.Rejected);
}
