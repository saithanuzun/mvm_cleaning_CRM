using MediatR;
using Microsoft.Extensions.Configuration;
using mvmclean.backend.Application.Services;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat.Enums;

namespace mvmclean.backend.Application.Features.Whatsapp.Commands;

public class HandleIncomingWhatsAppMessageRequest : IRequest<HandleIncomingWhatsAppMessageResponse>
{
    public string? MessageId { get; set; }
    public string From { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Timestamp { get; set; }
    public string? ConversationId { get; set; }
}

public class HandleIncomingWhatsAppMessageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Reply { get; set; }
    public string? MessageId { get; set; }
    public bool SentViaExternalApi { get; set; }
    public Guid? ChatId { get; set; }
}

public class HandleIncomingWhatsAppHandler : IRequestHandler<HandleIncomingWhatsAppMessageRequest, HandleIncomingWhatsAppMessageResponse>
{
    private readonly ILlmService _llmService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IWhatsAppChatRepository _whatsAppChatRepository;
    private readonly bool _autoSendReply;
    private readonly int _maxHistoryMessages;

    public HandleIncomingWhatsAppHandler(
        ILlmService llmService,
        IWhatsAppService whatsAppService,
        IWhatsAppChatRepository whatsAppChatRepository,
        IConfiguration configuration)
    {
        _llmService = llmService;
        _whatsAppService = whatsAppService;
        _whatsAppChatRepository = whatsAppChatRepository;
        _autoSendReply = true; //configuration.GetValue("WhatsApp:AutoSendReply", true);
        _maxHistoryMessages = 20; //configuration.GetValue("Llm:MaxHistoryMessages", 20);
    }

    public async Task<HandleIncomingWhatsAppMessageResponse> Handle(HandleIncomingWhatsAppMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.From))
        {
            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = false,
                Message = "Sender phone number (from) is required"
            };
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = false,
                Message = "Message text is required"
            };
        }

        var isNewChat = false;
        var chat = await _whatsAppChatRepository.GetByPhoneNumberAsync(request.From);
        if (chat == null)
        {
            chat = WhatsAppChat.Create(
                request.From,
                request.ContactName,
                request.ConversationId);
            isNewChat = true;
        }
        else
        {
            chat.UpdateContactName(request.ContactName);
            chat.SetExternalConversationId(request.ConversationId);
        }

        chat.AddUserMessage(request.Message, request.MessageId);

        var history = chat.GetRecentMessages(_maxHistoryMessages)
            .Select(m => new LlmChatHistoryMessage
            {
                Role = m.Role == ChatMessageRole.Assistant ? "assistant" : "user",
                Content = m.Content
            })
            .ToList();

        var llmResult = await _llmService.GenerateReplyAsync(new LlmChatRequest
        {
            UserMessage = request.Message,
            ContactName = chat.ContactName,
            PhoneNumber = chat.PhoneNumber.Value,
            ConversationId = request.ConversationId ?? request.MessageId,
            History = history
        }, cancellationToken);

        if (!llmResult.Success || string.IsNullOrWhiteSpace(llmResult.Reply))
        {
            await SaveChatAsync(chat, isNewChat);

            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = false,
                Message = llmResult.Message,
                MessageId = request.MessageId,
                ChatId = chat.Id
            };
        }

        var sentViaExternalApi = false;
        string? providerMessageId = null;

        if (_autoSendReply)
        {
            var sendResult = await _whatsAppService.SendMessageAsync(request.From, llmResult.Reply, cancellationToken);
            if (!sendResult.Success)
            {
                await SaveChatAsync(chat, isNewChat);

                return new HandleIncomingWhatsAppMessageResponse
                {
                    Success = false,
                    Message = sendResult.Message,
                    Reply = llmResult.Reply,
                    MessageId = request.MessageId,
                    ChatId = chat.Id
                };
            }

            sentViaExternalApi = true;
            providerMessageId = sendResult.ProviderMessageId;
        }

        chat.AddAssistantMessage(llmResult.Reply, providerMessageId);
        await SaveChatAsync(chat, isNewChat);

        return new HandleIncomingWhatsAppMessageResponse
        {
            Success = true,
            Message = "Incoming message processed and reply generated",
            Reply = llmResult.Reply,
            MessageId = request.MessageId,
            SentViaExternalApi = sentViaExternalApi,
            ChatId = chat.Id
        };
    }

    private async Task SaveChatAsync(WhatsAppChat chat, bool isNewChat)
    {
        if (isNewChat)
            await _whatsAppChatRepository.AddAsync(chat);
        else
            await _whatsAppChatRepository.UpdateAsync(chat);
    }
}
