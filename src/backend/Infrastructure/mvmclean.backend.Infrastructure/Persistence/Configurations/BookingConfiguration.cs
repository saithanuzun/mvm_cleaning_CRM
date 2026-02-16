using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Contractor;

namespace mvmclean.backend.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : EntityConfiguration<Booking>
{
    public override void Configure(EntityTypeBuilder<Booking> builder)
    {
        base.Configure(builder);

        builder.OwnsOne(i => i.TotalPrice);

        builder.OwnsOne(i => i.ServiceAddress, address =>
        {

            address.OwnsOne(a => a.Postcode);
        });

        builder.OwnsOne(i => i.ScheduledSlot);

        builder.OwnsMany(o => o.ServiceItems, li =>
        {
            li.OwnsOne(i => i.UnitAdjustedPrice);
            li.OwnsOne(i => i.OriginalPrice);
            li.Property(i => i.DiscountRate).HasColumnType("decimal(5,2)");
        });

        builder.HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(i => i.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber_Value")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.OwnsOne(i => i.Postcode, postcode =>
        {
            postcode.Property(p => p.Value)
                .HasColumnName("Postcode_Value")
                .HasMaxLength(10)
                .IsRequired();
            postcode.Property(p => p.Area)
                .HasColumnName("Postcode_Area")
                .HasMaxLength(2);
            postcode.Property(p => p.District)
                .HasColumnName("Postcode_District")
                .HasMaxLength(4);
            postcode.Property(p => p.Sector)
                .HasColumnName("Postcode_Sector")
                .HasMaxLength(8);
        });

        // Payment relationship
        builder.HasOne(b => b.Payment)
            .WithOne()
            .HasForeignKey<Booking>(b => b.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);

        // Contractor relationship
        builder.HasOne<Contractor>()
            .WithMany()
            .HasForeignKey(b => b.ContractorId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(b => b.ContractorId);
        builder.HasIndex(b => b.CustomerId);
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.CreationStatus);
    }
}