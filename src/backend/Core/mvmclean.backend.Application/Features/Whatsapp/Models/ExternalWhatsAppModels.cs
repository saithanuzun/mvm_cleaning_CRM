using System.Text.Json.Serialization;
using mvmclean.backend.Application.Features.Whatsapp.Commands;

namespace mvmclean.backend.Application.Features.Whatsapp.Models;

/// <summary>
/// Incoming webhook payload from the external WhatsApp API.
/// </summary>
public class ExternalWhatsAppIncomingRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("chat")]
    public string? Chat { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("isGroup")]
    public bool IsGroup { get; set; }

    public HandleIncomingWhatsAppMessageRequest ToCommandRequest()
    {
        var phone = WhatsAppJidHelper.ExtractPhoneNumber(From);
        var chatJid = string.IsNullOrWhiteSpace(Chat) ? From : Chat;

        return new HandleIncomingWhatsAppMessageRequest
        {
            MessageId = Id,
            From = From,
            PhoneNumber = phone,
            ChatJid = chatJid,
            Message = Text ?? string.Empty,
            UnixTimestamp = Timestamp,
            IsGroup = IsGroup,
            ConversationId = chatJid
        };
    }
}

/// <summary>
/// Reply returned to the external WhatsApp API (sync response path).
/// </summary>
public class ExternalWhatsAppIncomingReply
{
    public string Reply { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public bool SentViaExternalApi { get; set; }
    public bool AiTriggered { get; set; }
    public Guid? ChatId { get; set; }
}

public static class WhatsAppJidHelper
{
    public static string ExtractPhoneNumber(string jidOrPhone)
    {
        if (string.IsNullOrWhiteSpace(jidOrPhone))
            return string.Empty;

        var localPart = jidOrPhone.Split('@')[0];
        return new string(localPart.Where(char.IsDigit).ToArray());
    }

    public static bool IsWhatsAppJid(string value) =>
        value.Contains('@', StringComparison.Ordinal);
}
