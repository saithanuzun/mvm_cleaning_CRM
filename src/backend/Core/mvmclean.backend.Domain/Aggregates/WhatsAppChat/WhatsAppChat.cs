using mvmclean.backend.Domain.Aggregates.WhatsAppChat.Enums;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat.ValueObjects;
using mvmclean.backend.Domain.Core.BaseClasses;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;

namespace mvmclean.backend.Domain.Aggregates.WhatsAppChat;

public class WhatsAppChat : AggregateRoot
{
    public PhoneNumber PhoneNumber { get; set; }
    public string? ContactName { get; private set; }
    public string? ExternalConversationId { get; private set; }

    private readonly List<ChatMessage> _messages = new();
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private WhatsAppChat() { }

    public static WhatsAppChat Create(string phoneNumber, string? contactName = null, string? externalConversationId = null)
    {
        return new WhatsAppChat
        {
            PhoneNumber = PhoneNumber.Create(phoneNumber),
            ContactName = contactName,
            ExternalConversationId = externalConversationId
        };
    }

    public void UpdateContactName(string? contactName)
    {
        if (!string.IsNullOrWhiteSpace(contactName))
        {
            ContactName = contactName;
            MarkAsUpdated();
        }
    }

    public void SetExternalConversationId(string? externalConversationId)
    {
        if (!string.IsNullOrWhiteSpace(externalConversationId))
        {
            ExternalConversationId = externalConversationId;
            MarkAsUpdated();
        }
    }

    public ChatMessage AddUserMessage(string content, string? externalMessageId = null)
    {
        var message = new ChatMessage(ChatMessageRole.User, content, externalMessageId);
        _messages.Add(message);
        MarkAsUpdated();
        return message;
    }

    public ChatMessage AddAssistantMessage(string content, string? externalMessageId = null)
    {
        var message = new ChatMessage(ChatMessageRole.Assistant, content, externalMessageId);
        _messages.Add(message);
        MarkAsUpdated();
        return message;
    }

    public IReadOnlyList<ChatMessage> GetRecentMessages(int count)
    {
        return _messages
            .OrderBy(m => m.SentAt)
            .TakeLast(count)
            .ToList();
    }
}
