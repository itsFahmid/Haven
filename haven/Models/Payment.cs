using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Haven.Models;

[Table("Payments")]
public class Payment
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string Gateway { get; set; } = "bKash"; // bKash, Nagad, Rocket, SSLCommerz

    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = string.Empty;

    public bool OptInHallOfFame { get; set; } = false;

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
