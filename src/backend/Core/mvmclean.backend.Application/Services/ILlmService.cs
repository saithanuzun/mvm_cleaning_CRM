namespace mvmclean.backend.Application.Services;

public interface ILlmService
{
    Task<LlmCompletionResult> GenerateReplyAsync(LlmChatRequest request, CancellationToken cancellationToken = default);
}

public class LlmChatRequest
{
    public string UserMessage { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ConversationId { get; set; }
    public IReadOnlyList<LlmChatHistoryMessage> History { get; set; } = Array.Empty<LlmChatHistoryMessage>();
}

public class LlmChatHistoryMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class LlmCompletionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Reply { get; set; }
}
