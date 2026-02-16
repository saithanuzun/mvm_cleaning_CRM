using MediatR;
using mvmclean.backend.Application.Services;

namespace mvmclean.backend.Application.Features.Sms.Commands;

public class SendBulkSmsRequest : IRequest<SendBulkSmsResponse>
{
    public List<string> PhoneNumbers { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class SendBulkSmsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalNumbers { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> FailedNumbers { get; set; } = new();
}

public class SendBulkSmsHandler : IRequestHandler<SendBulkSmsRequest, SendBulkSmsResponse>
{
    private readonly ISmsService _smsService;

    public SendBulkSmsHandler(ISmsService smsService)
    {
        _smsService = smsService;
    }

    public async Task<SendBulkSmsResponse> Handle(SendBulkSmsRequest request, CancellationToken cancellationToken)
    {
        if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
        {
            return new SendBulkSmsResponse
            {
                Success = false,
                Message = "No phone numbers provided"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendBulkSmsResponse
            {
                Success = false,
                Message = "Message cannot be empty"
            };
        }

        var result = await _smsService.SendBulkSmsAsync(request.PhoneNumbers, request.Message);

        return new SendBulkSmsResponse
        {
            Success = result.IsSuccess,
            Message = result.Message,
            TotalNumbers = result.TotalNumbers,
            SuccessCount = result.SuccessCount,
            FailureCount = result.FailureCount,
            FailedNumbers = result.FailedNumbers
        };
    }
}
