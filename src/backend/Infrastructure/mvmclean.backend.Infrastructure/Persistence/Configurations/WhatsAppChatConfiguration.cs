using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;

namespace mvmclean.backend.Infrastructure.Persistence.Configurations;

public class WhatsAppChatConfiguration : EntityConfiguration<WhatsAppChat>
{
    public override void Configure(EntityTypeBuilder<WhatsAppChat> builder)
    {
        base.Configure(builder);
        
        builder.OwnsOne(i => i.PhoneNumber, phone =>
        {
        });

        builder.Property(c => c.ContactName)
            .HasMaxLength(256);

        builder.Property(c => c.ExternalConversationId)
            .HasMaxLength(128);

        builder.OwnsMany(c => c.Messages, messages =>
        {
            messages.Property(m => m.Role)
                .IsRequired();
            messages.Property(m => m.Content)
                .IsRequired();

            messages.Property(m => m.ExternalMessageId)
                .HasMaxLength(128);

            messages.Property(m => m.SentAt)
                .IsRequired();
        });

        builder.Navigation(c => c.Messages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
