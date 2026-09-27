using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obhoy.Data;
using Obhoy.Models;
using Obhoy.Services;

namespace Obhoy.Controllers;

public class PaymentController : Controller
{
    private readonly ObhoyDbContext _db;
    private readonly ISSLCommerzService _sslCommerz;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        ObhoyDbContext db,
        ISSLCommerzService sslCommerz,
        ILogger<PaymentController> logger)
    {
        _db = db;
        _sslCommerz = sslCommerz;
        _logger = logger;
    }

    // =========================================================================
    // 1. INITIATE MICRO-DONATION
    // =========================================================================
    [HttpPost]
    public async Task<IActionResult> InitiateDonation([FromBody] DonationSubmission submission)
    {
        if (submission == null || submission.AmountBDT < 10)
        {
            return BadRequest(new { success = false, message = "অনুগ্রহ করে কমপক্ষে ১০ টাকার পরিমাণ উল্লেখ করুন। / Minimum donation amount is ৳10." });
        }

        var tranId = $"OBH-DON-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
        var isAnonymous = submission.IsAnonymous;
        var donorName = isAnonymous 
            ? "Anonymous Hero" 
            : (string.IsNullOrWhiteSpace(submission.DonorName) ? "Kind Supporter" : submission.DonorName.Trim());

        int? currentUserId = GetCurrentUserId();

        var payment = new Payment
        {
            UserId = currentUserId > 0 ? currentUserId : null,
            Amount = submission.AmountBDT,
            Currency = "BDT",
            Gateway = "SSLCommerz",
            TransactionId = tranId,
            Status = PaymentStatus.Pending,
            Purpose = "Micro-Donation",
            IsAnonymous = isAnonymous,
            OptInHallOfFame = submission.OptIntoHallOfFame,
            DisplayName = donorName,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var initArgs = new PaymentInitiationArgs
        {
            TransactionId = tranId,
            Amount = submission.AmountBDT,
            Currency = "BDT",
            SuccessUrl = $"{baseUrl}/Payment/Success",
            FailUrl = $"{baseUrl}/Payment/Fail",
            CancelUrl = $"{baseUrl}/Payment/Cancel",
            IpnUrl = $"{baseUrl}/Payment/Ipn",
            CustomerName = isAnonymous ? "Haven Supporter" : donorName,
            CustomerEmail = "support@obhoy.org",
            CustomerPhone = string.IsNullOrWhiteSpace(submission.MobileNumber) ? "01700000000" : submission.MobileNumber,
            ProductName = "Obhoy Youth Care Sanctuary Donation",
            ProductCategory = "Donation",
            ValueA = "Micro-Donation",
            ValueB = donorName,
            ValueC = isAnonymous ? "1" : "0",
            ValueD = submission.OptIntoHallOfFame ? "1" : "0"
        };

        var initResult = await _sslCommerz.InitiatePaymentSessionAsync(initArgs);

        if (!initResult.Success || string.IsNullOrEmpty(initResult.GatewayPageURL))
        {
            _logger.LogError("SSLCommerz initialization failed for TranId {TranId}: {Error}", tranId, initResult.ErrorMessage);
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = initResult.ErrorMessage ?? "Could not connect to payment gateway";
            await _db.SaveChangesAsync();

            return StatusCode(502, new
            {
                success = false,
                message = "পেমেন্ট গেটওয়েতে সংযোগ করতে সমস্যা হয়েছে। অনুগ্রহ করে কিছুক্ষণ পর আবার চেষ্টা করুন। / Unable to establish connection with SSLCommerz gateway.",
                error = initResult.ErrorMessage
            });
        }

        return Json(new
        {
            success = true,
            gatewayUrl = initResult.GatewayPageURL,
            tranId = tranId,
            isMock = initResult.IsMock
        });
    }

    // =========================================================================
    // 2. INITIATE THERAPY BOOKING PAYMENT
    // =========================================================================
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InitiateBookingPayment(int bookingId)
    {
        int userId = GetCurrentUserId();
        var booking = await _db.Bookings
            .Include(b => b.Therapist)
                .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null)
        {
            TempData["ErrorMessage"] = "বুকিং রেকর্ড পাওয়া যায়নি। / Booking not found.";
            return RedirectToAction("MyBookings", "Booking");
        }

        if (booking.IsPaid)
        {
            TempData["InfoMessage"] = "এই বুকিংয়ের ফি ইতোমধ্যে পরিশোধিত হয়েছে। / Fee has already been settled.";
            return RedirectToAction("MyBookings", "Booking");
        }

        if (booking.FeeBDT <= 0 || booking.IsFeeSubsidized)
        {
            booking.IsPaid = true;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "এই সেশনটি বিনামূল্যে অথবা সম্পূর্ণ ভর্তুকিপ্রাপ্ত। / This session is fully subsidized.";
            return RedirectToAction("MyBookings", "Booking");
        }

        var tranId = $"OBH-BK-{booking.Id}-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var payment = new Payment
        {
            UserId = userId,
            BookingId = booking.Id,
            Amount = booking.FeeBDT,
            Currency = "BDT",
            Gateway = "SSLCommerz",
            TransactionId = tranId,
            Status = PaymentStatus.Pending,
            Purpose = "Therapy-Booking",
            IsAnonymous = false,
            DisplayName = User.Identity?.Name ?? "Haven Patient",
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var therapistName = booking.Therapist?.User?.FullName ?? "Specialist";

        var initArgs = new PaymentInitiationArgs
        {
            TransactionId = tranId,
            Amount = booking.FeeBDT,
            Currency = "BDT",
            SuccessUrl = $"{baseUrl}/Payment/Success",
            FailUrl = $"{baseUrl}/Payment/Fail",
            CancelUrl = $"{baseUrl}/Payment/Cancel",
            IpnUrl = $"{baseUrl}/Payment/Ipn",
            CustomerName = User.Identity?.Name ?? "Haven Patient",
            CustomerEmail = "support@obhoy.org",
            CustomerPhone = "01700000000",
            ProductName = $"Therapy Consultation ({therapistName})",
            ProductCategory = "Healthcare",
            ValueA = "Therapy-Booking",
            ValueB = booking.Id.ToString()
        };

        var initResult = await _sslCommerz.InitiatePaymentSessionAsync(initArgs);

        if (!initResult.Success || string.IsNullOrEmpty(initResult.GatewayPageURL))
        {
            TempData["ErrorMessage"] = "পেমেন্ট গেটওয়েতে সংযোগ করতে সমস্যা হয়েছে। অনুগ্রহ করে পুনরায় চেষ্টা করুন।";
            return RedirectToAction("MyBookings", "Booking");
        }

        return Redirect(initResult.GatewayPageURL);
    }

    // =========================================================================
    // 3. SECURE SUCCESS CALLBACK (SSLCommerz Form POST)
    // =========================================================================
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Success()
    {
        var tranId = Request.Form["tran_id"].ToString();
        var valId = Request.Form["val_id"].ToString();
        var cardType = Request.Form["card_type"].ToString();
        var bankTranId = Request.Form["bank_tran_id"].ToString();

        _logger.LogInformation("SSLCommerz Success callback received for TranId: {TranId}, ValId: {ValId}", tranId, valId);

        if (string.IsNullOrWhiteSpace(tranId))
        {
            return RedirectToAction(nameof(Fail), new { reason = "Missing transaction identifier" });
        }

        var payment = await _db.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b!.Therapist)
                    .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(p => p.TransactionId == tranId);

        if (payment == null)
        {
            _logger.LogWarning("Payment record not found for TranId: {TranId}", tranId);
            return RedirectToAction(nameof(Fail), new { tranId, reason = "Payment record not found in system" });
        }

        // Idempotency: If already completed, directly show receipt
        if (payment.Status == PaymentStatus.Completed)
        {
            return RedirectToAction(nameof(Receipt), new { tranId });
        }

        // CRITICAL SECURITY CHECK: Server-to-server validation query
        var validation = await _sslCommerz.ValidatePaymentAsync(valId);

        if (!validation.IsValid)
        {
            _logger.LogError("SSLCommerz Server Validation FAILED for TranId: {TranId}. Gateway reason: {Reason}", tranId, validation.ErrorMessage);
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = validation.ErrorMessage ?? "Server validation check failed";
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Fail), new { tranId, reason = "Security validation with gateway failed" });
        }

        // Validate Amount Match (Anti-Tampering)
        if (validation.Amount > 0 && Math.Abs(validation.Amount - payment.Amount) > 0.05m)
        {
            _logger.LogCritical("TAMPERING DETECTED: TranId {TranId} expected ৳{Expected} but gateway validated ৳{Actual}",
                tranId, payment.Amount, validation.Amount);

            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Tampered payment amount detected";
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Fail), new { tranId, reason = "Payment amount verification mismatch" });
        }

        // Mark payment as Completed
        payment.Status = PaymentStatus.Completed;
        payment.ValidationId = valId;
        payment.BankTransactionId = !string.IsNullOrWhiteSpace(validation.BankTransactionId) ? validation.BankTransactionId : bankTranId;
        payment.CardType = !string.IsNullOrWhiteSpace(validation.CardType) ? validation.CardType : cardType;
        payment.VerifiedAt = DateTime.UtcNow;

        // If linked to a booking, mark the booking as paid
        if (payment.BookingId.HasValue && payment.Booking != null)
        {
            payment.Booking.IsPaid = true;
            payment.Booking.PaymentId = payment.Id;
            payment.Booking.UpdatedAt = DateTime.UtcNow;
            _logger.LogInformation("Booking #{BookingId} marked as PAID for User {UserId}", payment.Booking.Id, payment.Booking.UserId);
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Receipt), new { tranId });
    }

    // =========================================================================
    // 4. FAIL CALLBACK
    // =========================================================================
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Fail()
    {
        var tranId = Request.Form["tran_id"].ToString();
        var failedReason = Request.Form["failedreason"].ToString();
        var error = Request.Form["error"].ToString();

        _logger.LogWarning("SSLCommerz Fail callback received for TranId: {TranId}. Reason: {Reason}", tranId, failedReason ?? error);

        if (!string.IsNullOrWhiteSpace(tranId))
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionId == tranId);
            if (payment != null && payment.Status != PaymentStatus.Completed)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = !string.IsNullOrWhiteSpace(failedReason) ? failedReason : error;
                await _db.SaveChangesAsync();
            }
        }

        return View("Failed", new PaymentReceiptViewModel
        {
            TransactionId = tranId,
            Status = PaymentStatus.Failed,
            MessageEn = "Your transaction could not be processed by the bank or gateway.",
            MessageBn = "ব্যাংক বা গেটওয়ে দ্বারা আপনার লেনদেনটি সম্পন্ন করা সম্ভব হয়নি।"
        });
    }

    [HttpGet]
    public IActionResult Failed(string? tranId, string? reason)
    {
        return View("Failed", new PaymentReceiptViewModel
        {
            TransactionId = tranId ?? string.Empty,
            Status = PaymentStatus.Failed,
            MessageEn = reason ?? "The payment transaction was unsuccessful.",
            MessageBn = "পেমেন্ট লেনদেনটি সফল হয়নি। আপনি চাইলে পুনরায় চেষ্টা করতে পারেন।"
        });
    }

    // =========================================================================
    // 5. CANCEL CALLBACK
    // =========================================================================
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Cancel()
    {
        var tranId = Request.Form["tran_id"].ToString();
        _logger.LogInformation("SSLCommerz Cancel callback received for TranId: {TranId}", tranId);

        if (!string.IsNullOrWhiteSpace(tranId))
        {
            var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionId == tranId);
            if (payment != null && payment.Status != PaymentStatus.Completed)
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.FailureReason = "Cancelled by user";
                await _db.SaveChangesAsync();

                if (payment.BookingId.HasValue)
                {
                    TempData["InfoMessage"] = "বুকিং পেমেন্ট বাতিল করা হয়েছে। আপনি পরবর্তীতে যেকোনো সময় ফি প্রদান করতে পারেন।";
                    return RedirectToAction("MyBookings", "Booking");
                }
            }
        }

        TempData["InfoMessage"] = "পেমেন্ট প্রক্রিয়াটি বাতিল করা হয়েছে। / Payment process was cancelled.";
        return RedirectToAction("Index", "Donate");
    }

    // =========================================================================
    // 6. ASYNC IPN (INSTANT PAYMENT NOTIFICATION) WEBHOOK
    // =========================================================================
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ipn()
    {
        var tranId = Request.Form["tran_id"].ToString();
        var valId = Request.Form["val_id"].ToString();

        _logger.LogInformation("SSLCommerz IPN Webhook triggered for TranId: {TranId}, ValId: {ValId}", tranId, valId);

        if (string.IsNullOrWhiteSpace(tranId) || string.IsNullOrWhiteSpace(valId))
        {
            return BadRequest(new { status = "INVALID_PAYLOAD" });
        }

        var payment = await _db.Payments
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.TransactionId == tranId);

        if (payment == null)
        {
            return NotFound(new { status = "TRANSACTION_NOT_FOUND" });
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            return Ok(new { status = "ALREADY_PROCESSED" });
        }

        var validation = await _sslCommerz.ValidatePaymentAsync(valId);
        if (validation.IsValid)
        {
            payment.Status = PaymentStatus.Completed;
            payment.ValidationId = valId;
            payment.BankTransactionId = validation.BankTransactionId;
            payment.CardType = validation.CardType;
            payment.VerifiedAt = DateTime.UtcNow;

            if (payment.BookingId.HasValue && payment.Booking != null)
            {
                payment.Booking.IsPaid = true;
                payment.Booking.PaymentId = payment.Id;
            }

            await _db.SaveChangesAsync();
            return Ok(new { status = "SUCCESS_PROCESSED" });
        }

        return BadRequest(new { status = "VALIDATION_FAILED" });
    }

    // =========================================================================
    // 7. VERIFIED RECEIPT DISPLAY
    // =========================================================================
    [HttpGet]
    public async Task<IActionResult> Receipt(string tranId)
    {
        if (string.IsNullOrWhiteSpace(tranId))
        {
            return RedirectToAction("Index", "Donate");
        }

        var payment = await _db.Payments
            .Include(p => p.Booking)
                .ThenInclude(b => b!.Therapist)
                    .ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(p => p.TransactionId == tranId);

        if (payment == null)
        {
            TempData["ErrorMessage"] = "পেমেন্ট রসিদ পাওয়া যায়নি। / Receipt record not found.";
            return RedirectToAction("Index", "Donate");
        }

        var viewModel = new PaymentReceiptViewModel
        {
            TransactionId = payment.TransactionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Gateway = payment.Gateway,
            CardType = string.IsNullOrWhiteSpace(payment.CardType) ? "bKash / Card (SSLCommerz)" : payment.CardType,
            BankTransactionId = payment.BankTransactionId ?? string.Empty,
            Purpose = payment.Purpose,
            DisplayName = payment.DisplayName ?? "Kind Supporter",
            IsAnonymous = payment.IsAnonymous,
            OptIntoHallOfFame = payment.OptInHallOfFame,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            VerifiedAt = payment.VerifiedAt,
            BookingReference = payment.Booking?.BookingReference,
            TherapistName = payment.Booking?.Therapist?.User?.FullName,
            MessageEn = payment.Purpose == "Therapy-Booking"
                ? $"Your therapy consultation booking fee of ৳{payment.Amount} has been verified and settled."
                : $"Thank you for your generous contribution of ৳{payment.Amount}! Your support directly empowers vulnerable youth across Bangladesh.",
            MessageBn = payment.Purpose == "Therapy-Booking"
                ? $"আপনার থেরাপি অ্যাপয়েন্টমেন্টের ফি ৳{payment.Amount} সফলভাবে পরিশোধিত ও যাচাইকৃত হয়েছে।"
                : $"আপনার ৳{payment.Amount} অনুদানের জন্য আন্তরিক কৃতজ্ঞতা! আপনার এই সহযোগিতা বিপদগ্রস্ত তরুণ ও শিশুদের সাইবার সুরক্ষা ও বিনামূল্যে মানসিক সহায়তায় সরাসরি ব্যয় হবে।"
        };

        return View(viewModel);
    }

    // =========================================================================
    // 8. SIMULATED GATEWAY CHECKOUT (SANDBOX / OFFLINE FALLBACK)
    // =========================================================================
    [HttpGet]
    public IActionResult MockCheckout(string tranId, decimal amount, string? purpose, string? fallbackReason)
    {
        ViewBag.TranId = tranId;
        ViewBag.Amount = amount;
        ViewBag.Purpose = purpose ?? "Obhoy Sanctuary Donation";
        ViewBag.FallbackReason = fallbackReason;
        return View();
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out int uid) ? uid : 0;
    }
}
