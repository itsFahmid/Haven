using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Obhoy.Models;

namespace Obhoy.Services;

public class SSLCommerzService : ISSLCommerzService
{
    private readonly HttpClient _httpClient;
    private readonly SSLCommerzSettings _settings;
    private readonly ILogger<SSLCommerzService> _logger;

    public SSLCommerzService(
        HttpClient httpClient,
        IOptions<SSLCommerzSettings> options,
        ILogger<SSLCommerzService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public bool IsMockMode() => _settings.UseMockFallback;

    public async Task<SSLCommerzInitResult> InitiatePaymentSessionAsync(PaymentInitiationArgs args)
    {
        if (_settings.UseMockFallback)
        {
            _logger.LogInformation("SSLCommerz Mock Mode active: Generating simulated gateway URL for TranId: {TranId}", args.TransactionId);
            return new SSLCommerzInitResult
            {
                Success = true,
                GatewayPageURL = $"/Payment/MockCheckout?tranId={Uri.EscapeDataString(args.TransactionId)}&amount={args.Amount}&purpose={Uri.EscapeDataString(args.ProductName)}",
                SessionKey = "MOCK-SESSION-" + Guid.NewGuid().ToString("N"),
                IsMock = true
            };
        }

        var postData = new Dictionary<string, string>
        {
            { "store_id", _settings.StoreId },
            { "store_passwd", _settings.StorePassword },
            { "total_amount", args.Amount.ToString("0.00", CultureInfo.InvariantCulture) },
            { "currency", string.IsNullOrWhiteSpace(args.Currency) ? "BDT" : args.Currency },
            { "tran_id", args.TransactionId },
            { "success_url", args.SuccessUrl },
            { "fail_url", args.FailUrl },
            { "cancel_url", args.CancelUrl },
            { "ipn_url", args.IpnUrl },

            // Customer Identity Isolation (Zero Tracking Principles)
            { "cus_name", string.IsNullOrWhiteSpace(args.CustomerName) ? "Haven Supporter" : args.CustomerName },
            { "cus_email", string.IsNullOrWhiteSpace(args.CustomerEmail) ? "support@obhoy.org" : args.CustomerEmail },
            { "cus_add1", string.IsNullOrWhiteSpace(args.CustomerAddress) ? "Dhaka, Bangladesh" : args.CustomerAddress },
            { "cus_city", string.IsNullOrWhiteSpace(args.CustomerCity) ? "Dhaka" : args.CustomerCity },
            { "cus_country", "Bangladesh" },
            { "cus_phone", string.IsNullOrWhiteSpace(args.CustomerPhone) ? "01700000000" : args.CustomerPhone },

            // Transaction Meta
            { "shipping_method", "NO" },
            { "num_of_item", "1" },
            { "product_name", string.IsNullOrWhiteSpace(args.ProductName) ? "Obhoy Safe Haven Support" : args.ProductName },
            { "product_category", string.IsNullOrWhiteSpace(args.ProductCategory) ? "Donation" : args.ProductCategory },
            { "product_profile", "non-physical-goods" },

            // Pass-through tracking attributes
            { "value_a", args.ValueA ?? string.Empty },
            { "value_b", args.ValueB ?? string.Empty },
            { "value_c", args.ValueC ?? string.Empty },
            { "value_d", args.ValueD ?? string.Empty }
        };

        try
        {
            _logger.LogInformation("Initiating SSLCommerz session for TranId: {TranId}, Amount: ৳{Amount}, Store: {StoreId}",
                args.TransactionId, args.Amount, _settings.StoreId);

            using var requestContent = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync(_settings.InitiationEndpoint, requestContent);

            var rawBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("SSLCommerz Raw Init Response: {Response}", rawBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("SSLCommerz HTTP error {StatusCode}: {Body}", response.StatusCode, rawBody);
                return FallbackToMockIfConfigured(args, $"SSLCommerz HTTP {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            string status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() ?? "" : "";

            if (status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                string gatewayUrl = root.TryGetProperty("GatewayPageURL", out var gwProp) ? gwProp.GetString() ?? "" : "";
                string sessionKey = root.TryGetProperty("sessionkey", out var sessProp) ? sessProp.GetString() ?? "" : "";

                return new SSLCommerzInitResult
                {
                    Success = true,
                    GatewayPageURL = gatewayUrl,
                    SessionKey = sessionKey,
                    IsMock = false
                };
            }
            else
            {
                string failedReason = root.TryGetProperty("failedreason", out var failProp) ? failProp.GetString() ?? "Session initiation rejected" : "Unknown error";
                _logger.LogWarning("SSLCommerz session initiation rejected for {TranId}: {Reason}", args.TransactionId, failedReason);
                return FallbackToMockIfConfigured(args, failedReason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception connecting to SSLCommerz gateway for {TranId}", args.TransactionId);
            return FallbackToMockIfConfigured(args, ex.Message);
        }
    }

    public async Task<SSLCommerzValidationResult> ValidatePaymentAsync(string valId)
    {
        if (string.IsNullOrWhiteSpace(valId))
        {
            return new SSLCommerzValidationResult
            {
                IsValid = false,
                ErrorMessage = "Missing validation id (val_id)."
            };
        }

        // Handle simulated mock checkout
        if (valId.StartsWith("MOCK-", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Validating Mock SSLCommerz payment for val_id: {ValId}", valId);
            return new SSLCommerzValidationResult
            {
                IsValid = true,
                Status = "VALID",
                ValidationId = valId,
                TransactionId = valId.Replace("MOCK-VAL-", ""),
                CardType = "MOCK-TEST-CHANNEL",
                BankTransactionId = "BNK-" + Random.Shared.Next(10000000, 99999999),
                Currency = "BDT"
            };
        }

        try
        {
            var queryUri = $"{_settings.ValidationEndpoint}?val_id={Uri.EscapeDataString(valId)}&store_id={Uri.EscapeDataString(_settings.StoreId)}&store_passwd={Uri.EscapeDataString(_settings.StorePassword)}&v=1&format=json";

            _logger.LogInformation("Querying SSLCommerz Validation Server for val_id: {ValId}", valId);
            var response = await _httpClient.GetAsync(queryUri);
            var rawBody = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("SSLCommerz Raw Validation Response: {Response}", rawBody);

            if (!response.IsSuccessStatusCode)
            {
                return new SSLCommerzValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Validation server HTTP {response.StatusCode}: {rawBody}"
                };
            }

            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;

            string status = root.TryGetProperty("status", out var sProp) ? sProp.GetString() ?? "" : "";
            bool isValid = status.Equals("VALID", StringComparison.OrdinalIgnoreCase) || status.Equals("VALIDATED", StringComparison.OrdinalIgnoreCase);

            decimal parsedAmount = 0;
            if (root.TryGetProperty("amount", out var amtProp))
            {
                if (amtProp.ValueKind == JsonValueKind.String)
                {
                    decimal.TryParse(amtProp.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedAmount);
                }
                else if (amtProp.ValueKind == JsonValueKind.Number)
                {
                    amtProp.TryGetDecimal(out parsedAmount);
                }
            }

            string tranId = root.TryGetProperty("tran_id", out var tProp) ? tProp.GetString() ?? "" : "";
            string currency = root.TryGetProperty("currency", out var cProp) ? cProp.GetString() ?? "BDT" : "BDT";
            string bankTranId = root.TryGetProperty("bank_tran_id", out var bProp) ? bProp.GetString() ?? "" : "";
            string cardType = root.TryGetProperty("card_type", out var ctProp) ? ctProp.GetString() ?? "" : "";
            string cardBrand = root.TryGetProperty("card_brand", out var cbProp) ? cbProp.GetString() ?? "" : "";
            string error = root.TryGetProperty("error", out var eProp) ? eProp.GetString() ?? "" : "";

            return new SSLCommerzValidationResult
            {
                IsValid = isValid,
                Status = status,
                TransactionId = tranId,
                ValidationId = valId,
                Amount = parsedAmount,
                Currency = currency,
                BankTransactionId = bankTranId,
                CardType = cardType,
                CardBrand = cardBrand,
                ErrorMessage = isValid ? null : (string.IsNullOrWhiteSpace(error) ? $"Gateway reported status '{status}'" : error)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query SSLCommerz validation server for val_id: {ValId}", valId);
            return new SSLCommerzValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Validation request failed: {ex.Message}"
            };
        }
    }

    private SSLCommerzInitResult FallbackToMockIfConfigured(PaymentInitiationArgs args, string reason)
    {
        // If developer enabled mock fallback or is testing offline, allow smooth local simulation
        if (_settings.UseMockFallback || _settings.IsSandbox)
        {
            _logger.LogWarning("SSLCommerz live initiation failed ({Reason}). Falling back to local sandbox simulator.", reason);
            return new SSLCommerzInitResult
            {
                Success = true,
                GatewayPageURL = $"/Payment/MockCheckout?tranId={Uri.EscapeDataString(args.TransactionId)}&amount={args.Amount}&purpose={Uri.EscapeDataString(args.ProductName)}&fallbackReason={Uri.EscapeDataString(reason)}",
                SessionKey = "MOCK-SESSION-" + Guid.NewGuid().ToString("N"),
                IsMock = true
            };
        }

        return new SSLCommerzInitResult
        {
            Success = false,
            ErrorMessage = reason
        };
    }
}
