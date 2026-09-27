using Obhoy.Models;

namespace Obhoy.Services;

public interface ISSLCommerzService
{
    Task<SSLCommerzInitResult> InitiatePaymentSessionAsync(PaymentInitiationArgs args);
    Task<SSLCommerzValidationResult> ValidatePaymentAsync(string valId);
    bool IsMockMode();
}
