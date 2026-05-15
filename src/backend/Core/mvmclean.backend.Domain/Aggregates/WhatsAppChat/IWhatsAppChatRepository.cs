using mvmclean.backend.Domain.Aggregates.WhatsAppChat.ValueObjects;
using mvmclean.backend.Domain.Core.Interfaces;

namespace mvmclean.backend.Domain.Aggregates.WhatsAppChat;

public interface IWhatsAppChatRepository : IGenericRepository<WhatsAppChat>
{
    Task<WhatsAppChat?> GetByPhoneNumberAsync(string phoneNumber, bool noTracking = false);

    Task<IReadOnlyList<ChatMessage>> GetRecentMessagesByPhoneAsync(string phoneNumber, int count);

    Task SaveChatAsync(WhatsAppChat chat, bool isNewChat);
}
