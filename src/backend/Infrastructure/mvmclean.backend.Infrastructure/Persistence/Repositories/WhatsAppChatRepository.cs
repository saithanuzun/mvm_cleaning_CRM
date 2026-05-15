using Microsoft.EntityFrameworkCore;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat.ValueObjects;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;

namespace mvmclean.backend.Infrastructure.Persistence.Repositories;

public class WhatsAppChatRepository : GenericRepository<WhatsAppChat>, IWhatsAppChatRepository
{
    private readonly MVMdbContext _dbContext;

    public WhatsAppChatRepository(MVMdbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WhatsAppChat?> GetByPhoneNumberAsync(string phoneNumber, bool noTracking = false)
    {
        var phone = PhoneNumber.Create(phoneNumber);

        var query = _dbContext.WhatsAppChats
            .Include(c => c.Messages)
            .Where(c => c.PhoneNumber.Value == phone.Value);

        if (noTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ChatMessage>> GetRecentMessagesByPhoneAsync(string phoneNumber, int count)
    {
        var phone = PhoneNumber.Create(phoneNumber);

        var messages = await _dbContext.WhatsAppChats
            .AsNoTracking()
            .Where(c => c.PhoneNumber.Value == phone.Value)
            .SelectMany(c => c.Messages)
            .OrderBy(m => m.SentAt)
            .Take(count)
            .ToListAsync();

        return messages;
    }

    public async Task SaveChatAsync(WhatsAppChat chat, bool isNewChat)
    {
        if (isNewChat)
        {
            await _dbContext.WhatsAppChats.AddAsync(chat);
        }

        await _dbContext.SaveChangesAsync();
    }
}
