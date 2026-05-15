namespace mvmclean.backend.Application.Services;

public interface IWhatsAppService
{
    Task<WhatsAppSendResult> SendMessageAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
}

public class WhatsAppSendResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ProviderMessageId { get; set; }
}
