using System.ComponentModel.DataAnnotations;

namespace Haven.Models;

public class BookingCreateModel
{
    [Required]
    public int TherapistId { get; set; }

    [Required(ErrorMessage = "অনুগ্রহ করে একটি সেশন তারিখ নির্বাচন করুন / Please select an appointment date.")]
    [DataType(DataType.Date)]
    public DateTime BookingDate { get; set; }

    [Required(ErrorMessage = "অনুগ্রহ করে একটি সময় স্লট নির্বাচন করুন / Please select a time slot.")]
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
