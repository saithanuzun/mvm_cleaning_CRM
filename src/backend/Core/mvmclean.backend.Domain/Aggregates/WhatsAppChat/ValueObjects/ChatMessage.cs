using mvmclean.backend.Domain.Aggregates.WhatsAppChat.Enums;
using mvmclean.backend.Domain.Core.BaseClasses;

namespace mvmclean.backend.Domain.Aggregates.WhatsAppChat.ValueObjects;

public class ChatMessage : ValueObject
{
    public Guid Id { get; private set; }
    public Guid WhatsAppChatId { get; private set; }
    public ChatMessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ExternalMessageId { get; private set; }
    public DateTime SentAt { get; private set; }

    public ChatMessage(ChatMessageRole role, string content, string? externalMessageId = null)
    {
        Id = Guid.NewGuid();
        Role = role;
        Content = content;
        ExternalMessageId = externalMessageId;
        SentAt = DateTime.UtcNow;
    }

    private ChatMessage() { }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
    }
}
