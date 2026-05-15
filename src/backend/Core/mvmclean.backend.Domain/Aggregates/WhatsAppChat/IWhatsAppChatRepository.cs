using mvmclean.backend.Domain.Core.Interfaces;

namespace mvmclean.backend.Domain.Aggregates.WhatsAppChat;

public interface IWhatsAppChatRepository : IGenericRepository<WhatsAppChat>
{
    Task<WhatsAppChat?> GetByPhoneNumberAsync(string phoneNumber, bool noTracking = false);
}
