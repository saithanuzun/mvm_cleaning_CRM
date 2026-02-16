namespace mvmclean.backend.Application.Services;

public interface ISmsService
{
    /// <summary>
    /// Sends an SMS message to a single phone number
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string message);

    /// <summary>
    /// Sends an SMS message to multiple phone numbers
    /// </summary>
    Task<BulkSmsResult> SendBulkSmsAsync(List<string> phoneNumbers, string message);
}

public class BulkSmsResult
{
    public int TotalNumbers { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedNumbers { get; set; } = new();
    public bool IsSuccess => FailureCount == 0;
    public string Message { get; set; } = string.Empty;
}
