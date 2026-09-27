namespace Obhoy.Models;

public class SSLCommerzSettings
{
    public const string SectionName = "SSLCommerz";

    public string StoreId { get; set; } = "testbox";
    public string StorePassword { get; set; } = "qwerty";
    public bool IsSandbox { get; set; } = true;
    public string BaseUrl { get; set; } = "https://sandbox.sslcommerz.com";
    public bool UseMockFallback { get; set; } = false;

    public string InitiationEndpoint => $"{BaseUrl.TrimEnd('/')}/gwprocess/v4/api.php";
    public string ValidationEndpoint => $"{BaseUrl.TrimEnd('/')}/validator/api/validationserver.php";
}

public class PaymentInitiationArgs
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string SuccessUrl { get; set; } = string.Empty;
    public string FailUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string CustomerName { get; set; } = "Haven Supporter";
    public string CustomerEmail { get; set; } = "donor@obhoy.org";
    public string CustomerPhone { get; set; } = "01700000000";
    public string CustomerAddress { get; set; } = "Dhaka, Bangladesh";
    public string CustomerCity { get; set; } = "Dhaka";
    public string CustomerCountry { get; set; } = "Bangladesh";
    public string ProductName { get; set; } = "Obhoy Youth Care Sanctuary";
    public string ProductCategory { get; set; } = "Donation";
    public string? ValueA { get; set; } // Purpose
    public string? ValueB { get; set; } // BookingId or Extra
    public string? ValueC { get; set; } // IsAnonymous
    public string? ValueD { get; set; } // OptInHallOfFame
}

public class SSLCommerzInitResult
{
    public bool Success { get; set; }
    public string? GatewayPageURL { get; set; }
    public string? SessionKey { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsMock { get; set; }
}

public class SSLCommerzValidationResult
{
    public bool IsValid { get; set; }
    public string? Status { get; set; } // VALID, VALIDATED, FAILED
    public string? TransactionId { get; set; }
    public string? ValidationId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? BankTransactionId { get; set; }
    public string? CardType { get; set; }
    public string? CardBrand { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentReceiptViewModel
{
    public string TransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BDT";
    public string Gateway { get; set; } = "SSLCommerz";
    public string CardType { get; set; } = "Mobile Banking / Card";
    public string BankTransactionId { get; set; } = string.Empty;
    public string Purpose { get; set; } = "Micro-Donation";
    public string DisplayName { get; set; } = "Anonymous Hero";
    public bool IsAnonymous { get; set; } = true;
    public bool OptIntoHallOfFame { get; set; } = false;
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public string? BookingReference { get; set; }
    public string? TherapistName { get; set; }
    public string? MessageEn { get; set; }
    public string? MessageBn { get; set; }
}
