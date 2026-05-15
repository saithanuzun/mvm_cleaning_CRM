using mvmclean.backend.Domain.Aggregates.WhatsAppChat.Enums;
using mvmclean.backend.Domain.Core.BaseClasses;

namespace mvmclean.backend.Domain.Aggregates.WhatsAppChat.ValueObjects;

public class ChatMessage : ValueObject
{
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ExternalMessageId { get; private set; }
    public DateTime SentAt { get; private set; }

    public ChatMessage(
        ChatMessageRole role,
        string content,
        string? externalMessageId = null,
        DateTime? sentAt = null)
    {
        Role = role;
        Content = content;
        ExternalMessageId = externalMessageId;
        SentAt = sentAt ?? DateTime.UtcNow;
    }

    private ChatMessage() { }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return null;
    }
}
