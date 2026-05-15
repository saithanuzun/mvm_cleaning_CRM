using MediatR;
using mvmclean.backend.Application.Features.Whatsapp.Models;
using mvmclean.backend.Application.Services;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;

namespace mvmclean.backend.Application.Features.Whatsapp.Commands;

public class SendWhatsAppMessageRequest : IRequest<SendWhatsAppMessageResponse>
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class SendWhatsAppMessageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ProviderMessageId { get; set; }
    public Guid? ChatId { get; set; }
}

public class SendWhatsAppMessageHandler : IRequestHandler<SendWhatsAppMessageRequest, SendWhatsAppMessageResponse>
{
    private readonly IWhatsAppService _whatsAppService;
    private readonly IWhatsAppChatRepository _whatsAppChatRepository;

    public SendWhatsAppMessageHandler(
        IWhatsAppService whatsAppService,
        IWhatsAppChatRepository whatsAppChatRepository)
    {
        _whatsAppService = whatsAppService;
        _whatsAppChatRepository = whatsAppChatRepository;
    }

    public async Task<SendWhatsAppMessageResponse> Handle(SendWhatsAppMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return new SendWhatsAppMessageResponse
            {
                Success = false,
                Message = "Phone number is required"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new SendWhatsAppMessageResponse
            {
                Success = false,
                Message = "Message cannot be empty"
            };
        }

        var phone = WhatsAppJidHelper.ExtractPhoneNumber(request.PhoneNumber);
        var result = await _whatsAppService.SendMessageAsync(request.PhoneNumber, request.Message, cancellationToken);

        if (!result.Success)
        {
            return new SendWhatsAppMessageResponse
            {
                Success = false,
                Message = result.Message
            };
        }

        var isNewChat = false;
        var chat = await _whatsAppChatRepository.GetByPhoneNumberAsync(phone);
        if (chat == null)
        {
            chat = WhatsAppChat.Create(phone);
            isNewChat = true;
        }

        chat.AddAssistantMessage(request.Message, result.ProviderMessageId);

        if (isNewChat)
            await _whatsAppChatRepository.AddAsync(chat);
        else
            await _whatsAppChatRepository.UpdateAsync(chat);

        return new SendWhatsAppMessageResponse
        {
            Success = true,
            Message = result.Message,
            ProviderMessageId = result.ProviderMessageId,
            ChatId = chat.Id
        };
    }
}
