using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Obhoy.Models;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}

[Table("Payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    public int? BookingId { get; set; }

    [ForeignKey(nameof(BookingId))]
    public Booking? Booking { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "BDT";

    [Required]
    [MaxLength(50)]
    public string Gateway { get; set; } = "SSLCommerz"; // SSLCommerz, bKash, Nagad, Rocket

    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty; // Unique merchant tran_id

    [MaxLength(100)]
    public string? ValidationId { get; set; } // val_id from SSLCommerz

    [MaxLength(100)]
    public string? BankTransactionId { get; set; } // bank_tran_id

    [MaxLength(50)]
    public string? CardType { get; set; } // e.g. BKASH-bKash, VISA-City Bank, NAGAD-Nagad

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(50)]
    public string Purpose { get; set; } = "Micro-Donation"; // "Micro-Donation", "Therapy-Booking"

    public bool IsAnonymous { get; set; } = true;

    public bool OptInHallOfFame { get; set; } = false;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }
}
