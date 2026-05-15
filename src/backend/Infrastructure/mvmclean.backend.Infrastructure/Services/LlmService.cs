using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Infrastructure.Services;

/// <summary>
/// Google Gemini generateContent API (https://generativelanguage.googleapis.com).
/// </summary>
public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmService> _logger;

    public LlmService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<LlmService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        var requestUrl = BuildGeminiRequestUrl();
        var systemPrompt = _configuration["Llm:SystemPrompt"]
            ?? "You are Emma, the WhatsApp assistant for MvM Cleaning. Use only information from https://www.mvmcleaning.com.";

        if (string.IsNullOrWhiteSpace(requestUrl))
        {
            _logger.LogWarning("Llm API URL is not configured; returning placeholder reply.");
            return new LlmCompletionResult
            {
                Success = true,
                Message = "LLM API not configured (placeholder reply)",
                Reply = $"Thanks for your message. We received: \"{request.UserMessage}\". A team member will follow up shortly."
            };
        }

        try
        {
            var payload = BuildGeminiPayload(request, systemPrompt);
            var json = JsonSerializer.Serialize(payload);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            _logger.LogDebug("Sending Gemini request to {Url}", MaskApiKeyInUrl(requestUrl));

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Gemini API returned {StatusCode}. Body: {Body}",
                    (int)response.StatusCode, responseBody);

                return new LlmCompletionResult
                {
                    Success = false,
                    Message = $"Gemini API error: {(int)response.StatusCode} {response.ReasonPhrase}"
                };
            }

            var reply = ExtractGeminiReply(responseBody);
            if (string.IsNullOrWhiteSpace(reply))
            {
                return new LlmCompletionResult
                {
                    Success = false,
                    Message = "Gemini API returned an empty reply"
                };
            }

            return new LlmCompletionResult
            {
                Success = true,
                Message = "Reply generated",
                Reply = reply.Trim()
            };
        }
        catch (OperationCanceledException)
        {
            return new LlmCompletionResult
            {
                Success = false,
                Message = "LLM request was cancelled"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API.");
            return new LlmCompletionResult
            {
                Success = false,
                Message = $"Failed to call Gemini API: {ex.Message}"
            };
        }
    }

    private string? BuildGeminiRequestUrl()
    {
        var apiUrl = _configuration["Llm:ApiUrl"];
        if (!string.IsNullOrWhiteSpace(apiUrl))
            return apiUrl;

        var apiBaseUrl = _configuration["Llm:ApiBaseUrl"]
            ?? "https://generativelanguage.googleapis.com/v1beta/models";
        var model = _configuration["Llm:Model"] ?? "gemini-2.5-flash";
        var apiKey = _configuration["Llm:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        return $"{apiBaseUrl.TrimEnd('/')}/{model}:generateContent?key={apiKey}";
    }

    private static object BuildGeminiPayload(LlmChatRequest request, string systemPrompt)
    {
        var contents = new List<object>();

        if (request.History.Count > 0)
        {
            foreach (var item in request.History)
            {
                if (string.IsNullOrWhiteSpace(item.Content))
                    continue;

                var role = item.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                    || item.Role.Equals("model", StringComparison.OrdinalIgnoreCase)
                    ? "model"
                    : "user";

                contents.Add(new
                {
                    role,
                    parts = new[] { new { text = item.Content } }
                });
            }
        }
        else
        {
            contents.Add(new
            {
                parts = new[] { new { text = BuildUserContent(request) } }
            });
        }

        return new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents
        };
    }

    private static string BuildUserContent(LlmChatRequest request)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.ContactName))
            parts.Add($"Customer name: {request.ContactName}");

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            parts.Add($"Phone: {request.PhoneNumber}");

        parts.Add(request.UserMessage);

        return string.Join("\n", parts);
    }

    private static string? ExtractGeminiReply(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Gemini: candidates[0].content.parts[0].text
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];
                if (first.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }
            }

            // Fallback for other providers
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("content", out var content))
                    return content.GetString();
            }
        }
        catch (JsonException)
        {
            // ignore
        }

        return null;
    }

    private static string MaskApiKeyInUrl(string url)
    {
        const string keyParam = "key=";
        var idx = url.IndexOf(keyParam, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return url;

        return url[..(idx + keyParam.Length)] + "***";
    }
}
