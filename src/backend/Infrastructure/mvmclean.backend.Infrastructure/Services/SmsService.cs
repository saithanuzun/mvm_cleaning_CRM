using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Infrastructure.Services;

/// <summary>
/// Placeholder SMS service implementation
/// This logs SMS actions but doesn't actually send SMS yet
/// Implement actual SMS provider (Twilio, AWS SNS, etc.) later
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an SMS message to a single phone number
    /// Currently logs the action - implement actual SMS provider later
    /// </summary>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation(
                "SMS would be sent to {PhoneNumber}. Message: {Message}",
                phoneNumber, message);

            // TODO: Implement actual SMS sending logic here
            // Example with Twilio:
            // var messageResource = await MessageResource.CreateAsync(
            //     body: message,
            //     from: new PhoneNumber(_twilioPhoneNumber),
            //     to: new PhoneNumber(phoneNumber)
            // );

            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send SMS to {PhoneNumber}",
                phoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Sends an SMS message to multiple phone numbers
    /// Currently logs the action - implement actual SMS provider later
    /// </summary>
    public async Task<BulkSmsResult> SendBulkSmsAsync(List<string> phoneNumbers, string message)
    {
        var result = new BulkSmsResult
        {
            TotalNumbers = phoneNumbers.Count,
            SuccessCount = 0,
            FailureCount = 0,
            FailedNumbers = new List<string>()
        };

        _logger.LogInformation(
            "Bulk SMS would be sent to {Count} phone numbers. Message: {Message}",
            phoneNumbers.Count, message);

        foreach (var phoneNumber in phoneNumbers)
        {
            var success = await SendSmsAsync(phoneNumber, message);

            if (success)
            {
                result.SuccessCount++;
            }
            else
            {
                result.FailureCount++;
                result.FailedNumbers.Add(phoneNumber);
            }
        }

        result.Message = result.IsSuccess
            ? $"Successfully sent SMS to all {result.SuccessCount} numbers"
            : $"Sent SMS to {result.SuccessCount} numbers, failed for {result.FailureCount} numbers";

        _logger.LogInformation(
            "Bulk SMS completed: {SuccessCount} successful, {FailureCount} failed",
            result.SuccessCount, result.FailureCount);

        return result;
    }
}
