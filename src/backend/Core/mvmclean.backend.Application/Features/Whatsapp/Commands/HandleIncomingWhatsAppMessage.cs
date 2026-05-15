using MediatR;
using Microsoft.Extensions.Configuration;
using mvmclean.backend.Application.Features.Whatsapp.Models;
using mvmclean.backend.Application.Services;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat.Enums;

namespace mvmclean.backend.Application.Features.Whatsapp.Commands;

public class HandleIncomingWhatsAppMessageRequest : IRequest<HandleIncomingWhatsAppMessageResponse>
{
    public string? MessageId { get; set; }
    public string From { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string ChatJid { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public long UnixTimestamp { get; set; }
    public bool IsGroup { get; set; }
    public string? ConversationId { get; set; }
}

public class HandleIncomingWhatsAppMessageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Reply { get; set; }
    public string? MessageId { get; set; }
    public bool SentViaExternalApi { get; set; }
    public bool AiTriggered { get; set; }
    public Guid? ChatId { get; set; }
}

public class HandleIncomingWhatsAppHandler : IRequestHandler<HandleIncomingWhatsAppMessageRequest, HandleIncomingWhatsAppMessageResponse>
{
    private readonly ILlmService _llmService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IWhatsAppChatRepository _whatsAppChatRepository;
    private readonly bool _autoSendReply;
    private readonly int _maxHistoryMessages;
    private readonly string _aiTriggerPrefix;
    private readonly string _defaultBotReply;

    public HandleIncomingWhatsAppHandler(
        ILlmService llmService,
        IWhatsAppService whatsAppService,
        IWhatsAppChatRepository whatsAppChatRepository,
        IConfiguration configuration)
    {
        _llmService = llmService;
        _whatsAppService = whatsAppService;
        _whatsAppChatRepository = whatsAppChatRepository;
        _autoSendReply = configuration.GetValue("WhatsApp:AutoSendReply", true);
        _maxHistoryMessages = configuration.GetValue("Llm:MaxHistoryMessages", 20);
        _aiTriggerPrefix = configuration["WhatsApp:AiTriggerPrefix"] ?? "emma";
        _defaultBotReply = configuration["WhatsApp:DefaultBotReply"]
            ?? "Hi! Start your message with \"emma\" to chat with our AI assistant.";
    }

    public async Task<HandleIncomingWhatsAppMessageResponse> Handle(HandleIncomingWhatsAppMessageRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.From))
        {
            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = false,
                Message = "Sender (from) is required"
            };
        }

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = false,
                Message = "Could not extract phone number from sender"
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
        var chat = await _whatsAppChatRepository.GetByPhoneNumberAsync(request.PhoneNumber);
        if (chat == null)
        {
            chat = WhatsAppChat.Create(
                request.PhoneNumber,
                externalConversationId: request.ChatJid,
                isGroup: request.IsGroup);
            isNewChat = true;
        }
        else
        {
            chat.SetExternalConversationId(request.ChatJid);
            chat.SetIsGroup(request.IsGroup);
        }

        chat.AddUserMessage(request.Message, request.MessageId, ToUtc(request.UnixTimestamp));

        if (!ShouldTriggerAi(request.Message))
        {
            var botReply = _defaultBotReply;
            chat.AddAssistantMessage(botReply);
            await SaveChatAsync(chat, isNewChat);

            var sent = await TrySendReplyAsync(request.From, botReply, cancellationToken);

            return new HandleIncomingWhatsAppMessageResponse
            {
                Success = true,
                Message = "Message stored; default bot reply sent",
                Reply = botReply,
                MessageId = request.MessageId,
                SentViaExternalApi = sent.Sent,
                AiTriggered = false,
                ChatId = chat.Id
            };
        }

        var promptForAi = StripAiTriggerPrefix(request.Message);
        var history = chat.GetRecentMessages(_maxHistoryMessages)
            .Select(m => new LlmChatHistoryMessage
            {
                Role = m.Role == ChatMessageRole.Assistant ? "assistant" : "user",
                Content = m.Content
            })
            .ToList();

        var llmResult = await _llmService.GenerateReplyAsync(new LlmChatRequest
        {
            UserMessage = promptForAi,
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
                AiTriggered = true,
                ChatId = chat.Id
            };
        }

        chat.AddAssistantMessage(llmResult.Reply);
        await SaveChatAsync(chat, isNewChat);

        var sendResult = await TrySendReplyAsync(request.From, llmResult.Reply, cancellationToken);

        return new HandleIncomingWhatsAppMessageResponse
        {
            Success = true,
            Message = "Incoming message processed; AI reply generated",
            Reply = llmResult.Reply,
            MessageId = request.MessageId,
            SentViaExternalApi = sendResult.Sent,
            AiTriggered = true,
            ChatId = chat.Id
        };
    }

    private bool ShouldTriggerAi(string message)
    {
        var trimmed = message.TrimStart();
        return trimmed.StartsWith(_aiTriggerPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private string StripAiTriggerPrefix(string message)
    {
        var trimmed = message.TrimStart();
        if (!trimmed.StartsWith(_aiTriggerPrefix, StringComparison.OrdinalIgnoreCase))
            return message;

        return trimmed[_aiTriggerPrefix.Length..].TrimStart();
    }

    private async Task<(bool Sent, string? ProviderMessageId)> TrySendReplyAsync(
        string recipient,
        string reply,
        CancellationToken cancellationToken)
    {
        if (!_autoSendReply || string.IsNullOrWhiteSpace(reply))
            return (false, null);

        var sendResult = await _whatsAppService.SendMessageAsync(recipient, reply, cancellationToken);
        return (sendResult.Success, sendResult.ProviderMessageId);
    }

    private static DateTime? ToUtc(long unixTimestamp) =>
        unixTimestamp > 0
            ? DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime
            : null;

    private async Task SaveChatAsync(WhatsAppChat chat, bool isNewChat)
    {
        if (isNewChat)
            await _whatsAppChatRepository.AddAsync(chat);
        else
            await _whatsAppChatRepository.UpdateAsync(chat);
    }
}
