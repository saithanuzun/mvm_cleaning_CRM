using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Features.Whatsapp.Models;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Infrastructure.Services;

/// <summary>
/// Sends messages via the external WhatsApp API (not Meta Cloud API directly).
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;

    private bool _clientInitialized = false;
    private readonly object _initLock = new();

    public WhatsAppService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WhatsAppService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WhatsAppSendResult> SendMessageAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(toPhoneNumber))
            throw new ArgumentException("Phone number must not be empty.", nameof(toPhoneNumber));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message must not be empty.", nameof(message));

        var baseUrl  = _configuration["WhatsApp:ExternalApiBaseUrl"];
        var sendPath = _configuration["WhatsApp:SendPath"] ?? "messages/send";
        var apiKey   = _configuration["WhatsApp:ApiKey"];

        var to = WhatsAppJidHelper.IsWhatsAppJid(toPhoneNumber)
            ? toPhoneNumber
            : NormalizePhoneNumber(toPhoneNumber);

        // ── Dev / staging: no external URL configured ──────────────────────────
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogInformation(
                "WhatsApp send skipped (ExternalApiBaseUrl not configured). " +
                "Recipient: {PhoneNumber} | Message: {Message}",
                to, message);

            return new WhatsAppSendResult
            {
                Success = true,
                Message = "WhatsApp send logged (ExternalApiBaseUrl not configured)"
            };
        }

        // ── One-time client initialisation (BaseAddress + auth header) ─────────
        EnsureClientInitialized(baseUrl, apiKey);

        // ── Send ───────────────────────────────────────────────────────────────
        try
        {
            var payload = new
            {
                to,
                phoneNumber = WhatsAppJidHelper.IsWhatsAppJid(to)
                    ? WhatsAppJidHelper.ExtractPhoneNumber(to)
                    : to,
                message
            };

            _logger.LogDebug(
                "Sending WhatsApp message to {PhoneNumber} via {BaseUrl}{Path}",
                to, baseUrl, sendPath);

            var response = await _httpClient.PostAsJsonAsync(
                sendPath.TrimStart('/'), payload, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "WhatsApp API returned {StatusCode} for {PhoneNumber}. Body: {Body}",
                    (int)response.StatusCode, to, responseBody);

                return new WhatsAppSendResult
                {
                    Success = false,
                    Message = $"External WhatsApp API error: {(int)response.StatusCode} {response.ReasonPhrase}"
                };
            }

            var providerMessageId = TryExtractMessageId(responseBody);

            _logger.LogInformation(
                "WhatsApp message sent to {PhoneNumber}. ProviderMessageId: {ProviderMessageId}",
                to, providerMessageId ?? "n/a");

            return new WhatsAppSendResult
            {
                Success           = true,
                Message           = "Message sent successfully",
                ProviderMessageId = providerMessageId
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("WhatsApp send to {Recipient} was cancelled.", to);
            return new WhatsAppSendResult
            {
                Success = false,
                Message = "Send operation was cancelled"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "HTTP request failed while sending WhatsApp message to {Recipient}.", to);

            return new WhatsAppSendResult
            {
                Success = false,
                Message = $"HTTP request failed: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error while sending WhatsApp message to {Recipient}.", to);

            return new WhatsAppSendResult
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}"
            };
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets BaseAddress and the API-key header once, thread-safely.
    /// Mutating BaseAddress after the first request throws, so we guard with a flag.
    /// </summary>
    private void EnsureClientInitialized(string baseUrl, string? apiKey)
    {
        if (_clientInitialized) return;

        lock (_initLock)
        {
            if (_clientInitialized) return;

            _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + '/');

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Remove("X-Api-Key");
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
            }

            _clientInitialized = true;
        }
    }

    /// <summary>
    /// Attempts to extract a message ID from the API response JSON.
    /// Tries common field names: "messageId", "id", "message_id".
    /// Returns null if not found or on parse error.
    /// </summary>
    private static string? TryExtractMessageId(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return null;

        try
        {
            using var doc  = JsonDocument.Parse(responseBody);
            var       root = doc.RootElement;

            foreach (var key in new[] { "messageId", "id", "message_id" })
            {
                if (root.TryGetProperty(key, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    return value.GetString();
                }
            }
        }
        catch (JsonException ex)
        {
            // Non-JSON response body — not an error worth surfacing
            // (already logged at call site if status code was bad)
            _ = ex;
        }

        return null;
    }

    /// <summary>
    /// Strips all non-digit characters from a phone number.
    /// E.g. "+1 (555) 123-4567" → "15551234567"
    /// </summary>
    private static string NormalizePhoneNumber(string phoneNumber) =>
        new(phoneNumber.Where(char.IsDigit).ToArray());
}