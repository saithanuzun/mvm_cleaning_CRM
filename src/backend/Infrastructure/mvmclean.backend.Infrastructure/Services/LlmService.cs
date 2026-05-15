using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Infrastructure.Services;

/// <summary>
/// Calls an external LLM HTTP API (OpenAI-compatible chat completions by default).
/// </summary>
public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmService> _logger;

    private bool _clientInitialized = false;
    private readonly object _initLock = new();

    public LlmService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LlmService> logger)
    {
        _httpClient    = httpClient    ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LlmCompletionResult> GenerateReplyAsync(
        LlmChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return new LlmCompletionResult
            {
                Success = false,
                Message = "User message is required"
            };
        }

        var apiUrl      = _configuration["Llm:ApiUrl"];
        var apiKey      = _configuration["Llm:ApiKey"];
        var model       = _configuration["Llm:Model"] ?? "gemini";
        var systemPrompt = _configuration["Llm:SystemPrompt"]
            ?? "You are Emma, the WhatsApp assistant for MvM Cleaning, a professional carpet, sofa, and upholstery cleaning company in the UK. Answer concisely. For all services, pricing, coverage, and booking information, use only accurate details from https://www.mvmcleaning.com — do not invent prices or services. If unsure, offer to have a team member follow up.";

        // ── Dev / staging: no API URL configured ──────────────────────────────
        if (string.IsNullOrWhiteSpace(apiUrl))
        {
            _logger.LogWarning("Llm:ApiUrl is not configured; returning placeholder reply.");
            return new LlmCompletionResult
            {
                Success = true,
                Message = "LLM API not configured (placeholder reply)",
                Reply   = $"Thanks for your message. We received: \"{request.UserMessage}\". A team member will follow up shortly."
            };
        }

        // ── One-time client initialisation (BaseAddress + auth header) ─────────
        EnsureClientInitialized(apiUrl, apiKey);

        // ── Send ───────────────────────────────────────────────────────────────
        try
        {
            var payload = new
            {
                model,
                messages = BuildChatMessages(request, systemPrompt)
            };

            _logger.LogDebug("Sending LLM request to {ApiUrl} with model {Model}.", apiUrl, model);

            var response = await _httpClient.PostAsJsonAsync(
                string.Empty, payload, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "LLM API returned {StatusCode} {ReasonPhrase}. Body: {Body}",
                    (int)response.StatusCode, response.ReasonPhrase, responseBody);

                return new LlmCompletionResult
                {
                    Success = false,
                    Message = $"LLM API error: {(int)response.StatusCode} {response.ReasonPhrase}"
                };
            }

            var reply = ExtractReplyFromResponse(responseBody);

            if (string.IsNullOrWhiteSpace(reply))
            {
                _logger.LogWarning("LLM API responded successfully but returned an empty reply.");
                return new LlmCompletionResult
                {
                    Success = false,
                    Message = "LLM API returned an empty reply"
                };
            }

            _logger.LogInformation("LLM reply generated successfully.");

            return new LlmCompletionResult
            {
                Success = true,
                Message = "Reply generated",
                Reply   = reply.Trim()
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("LLM API request was cancelled.");
            return new LlmCompletionResult
            {
                Success = false,
                Message = "LLM request was cancelled"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while calling LLM API.");
            return new LlmCompletionResult
            {
                Success = false,
                Message = $"HTTP request failed: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling LLM API.");
            return new LlmCompletionResult
            {
                Success = false,
                Message = $"Failed to call LLM API: {ex.Message}"
            };
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets BaseAddress and the Bearer auth header once, thread-safely.
    /// Mutating BaseAddress after the first request throws, so we guard with a flag.
    /// </summary>
    private void EnsureClientInitialized(string apiUrl, string? apiKey)
    {
        if (_clientInitialized) return;

        lock (_initLock)
        {
            if (_clientInitialized) return;

            _httpClient.BaseAddress = new Uri(apiUrl);

            if (!string.IsNullOrWhiteSpace(apiKey))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

            _clientInitialized = true;
        }
    }

    /// <summary>
    /// Builds the messages array for the chat completion request.
    /// If the request has history, each item is mapped to a role/content pair.
    /// Otherwise a single user message is built from the request fields.
    /// </summary>
    private static object[] BuildChatMessages(LlmChatRequest request, string systemPrompt)
    {
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        if (request.History.Count > 0)
        {
            foreach (var item in request.History)
            {
                if (string.IsNullOrWhiteSpace(item.Content))
                    continue;

                var role = item.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                    ? "assistant"
                    : "user";

                messages.Add(new { role, content = item.Content });
            }
        }
        else
        {
            messages.Add(new { role = "user", content = BuildUserContent(request) });
        }

        return messages.ToArray();
    }

    /// <summary>
    /// Builds a plain-text user message that includes available contact context.
    /// </summary>
    private static string BuildUserContent(LlmChatRequest request)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.ContactName))
            parts.Add($"Customer name: {request.ContactName}");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            parts.Add($"Phone: {request.PhoneNumber}");

        parts.Add($"Message: {request.UserMessage}");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Parses the LLM API response body and extracts the reply text.
    /// Supports OpenAI-compatible format first, then common fallback keys.
    /// Returns null if no reply text could be found.
    /// </summary>
    private static string? ExtractReplyFromResponse(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return null;

        try
        {
            using var doc  = JsonDocument.Parse(responseBody);
            var       root = doc.RootElement;

            // OpenAI-compatible: choices[0].message.content
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];

                if (first.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var content))
                    return content.GetString();

                if (first.TryGetProperty("text", out var text))
                    return text.GetString();
            }

            // Generic fallback keys
            foreach (var key in new[] { "reply", "response", "output", "text", "content" })
            {
                if (root.TryGetProperty(key, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                    return value.GetString();
            }
        }
        catch (JsonException ex)
        {
            // Non-JSON or malformed body — caller handles the null return
            _ = ex;
        }

        return null;
    }
}