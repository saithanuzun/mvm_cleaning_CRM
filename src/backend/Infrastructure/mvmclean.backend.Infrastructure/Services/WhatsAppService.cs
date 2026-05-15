using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Features.Whatsapp.Models;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Infrastructure.Services;

/// <summary>
/// POST {baseUrl}/messages/send with body: {"to":"447862265412","text":"..."}
/// </summary>
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;

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

        var baseUrl = _configuration["WhatsApp:ExternalApiBaseUrl"];
        var sendPath = _configuration["WhatsApp:SendPath"] ?? "messages/send";
        var apiKey = _configuration["WhatsApp:ApiKey"];

        var to = WhatsAppJidHelper.IsWhatsAppJid(toPhoneNumber)
            ? WhatsAppJidHelper.ExtractPhoneNumber(toPhoneNumber)
            : NormalizePhoneNumber(toPhoneNumber);

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogInformation(
                "WhatsApp send skipped (ExternalApiBaseUrl not configured). To: {To} | Text: {Text}",
                to, message);

            return new WhatsAppSendResult
            {
                Success = true,
                Message = "WhatsApp send logged (ExternalApiBaseUrl not configured)"
            };
        }

        var requestUri = new Uri($"{baseUrl.TrimEnd('/')}/{sendPath.TrimStart('/')}");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = JsonContent.Create(new { to, text = message });

            if (!string.IsNullOrWhiteSpace(apiKey))
                request.Headers.Add("X-API-Key", apiKey);

            _logger.LogDebug("POST {Uri} to={To}", requestUri, to);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "WhatsApp API {StatusCode} for {To}. Body: {Body}",
                    (int)response.StatusCode, to, responseBody);

                return new WhatsAppSendResult
                {
                    Success = false,
                    Message = $"WhatsApp API error: {(int)response.StatusCode}"
                };
            }

            var providerMessageId = TryExtractMessageId(responseBody);

            _logger.LogInformation(
                "WhatsApp sent to {To}. Id: {Id}",
                to, providerMessageId ?? "n/a");

            return new WhatsAppSendResult
            {
                Success = true,
                Message = "Message sent successfully",
                ProviderMessageId = providerMessageId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp message to {To}", to);
            return new WhatsAppSendResult
            {
                Success = false,
                Message = $"Failed to send WhatsApp message: {ex.Message}"
            };
        }
    }

    private static string? TryExtractMessageId(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
                return id.GetString();
        }
        catch (JsonException)
        {
            // ignore
        }

        return null;
    }

    private static string NormalizePhoneNumber(string phoneNumber) =>
        new(phoneNumber.Where(char.IsDigit).ToArray());
}
