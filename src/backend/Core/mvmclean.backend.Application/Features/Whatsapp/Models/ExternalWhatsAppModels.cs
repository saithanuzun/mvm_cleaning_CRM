namespace mvmclean.backend.Application.Features.Whatsapp.Models;

/// <summary>
/// Payload sent by the external WhatsApp API into this project.
/// </summary>
public class ExternalWhatsAppIncomingRequest
{
    public string? MessageId { get; set; }
    public string From { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Timestamp { get; set; }
    public string? ConversationId { get; set; }
}

/// <summary>
/// Reply returned to the external WhatsApp API (sync response path).
/// </summary>
public class ExternalWhatsAppIncomingReply
{
    public string Reply { get; set; } = string.Empty;
    public string? MessageId { get; set; }
    public bool SentViaExternalApi { get; set; }
}
