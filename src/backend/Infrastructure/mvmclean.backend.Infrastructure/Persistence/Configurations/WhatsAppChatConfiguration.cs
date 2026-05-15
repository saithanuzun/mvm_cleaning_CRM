using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;

namespace mvmclean.backend.Infrastructure.Persistence.Configurations;

public class WhatsAppChatConfiguration : EntityConfiguration<WhatsAppChat>
{
    public override void Configure(EntityTypeBuilder<WhatsAppChat> builder)
    {
        base.Configure(builder);

        builder.ToTable("WhatsAppChats");

        builder.OwnsOne(c => c.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber_Value")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.HasIndex("PhoneNumber_Value")
            .IsUnique();

        builder.Property(c => c.ContactName)
            .HasMaxLength(256);

        builder.Property(c => c.ExternalConversationId)
            .HasMaxLength(128);

        builder.Property(c => c.IsGroup)
            .IsRequired()
            .HasDefaultValue(false);

        builder.OwnsMany(c => c.Messages, messages =>
        {
            messages.ToTable("ChatMessage");
            messages.WithOwner().HasForeignKey("WhatsAppChatId");
            messages.HasKey("WhatsAppChatId", "Id");

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
