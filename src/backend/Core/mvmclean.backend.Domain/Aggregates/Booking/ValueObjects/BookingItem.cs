using mvmclean.backend.Domain.Core.BaseClasses;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;

namespace mvmclean.backend.Domain.Aggregates.Booking.ValueObjects;

public class BookingItem : ValueObject
{
    public string ServiceName { get; set; }
    public Guid ServiceId { get; set; }
    public Money UnitAdjustedPrice { get; set; } // This is the discounted price
    public Money? OriginalPrice { get; set; } // Store original price before discount
    public decimal DiscountRate { get; set; } // Discount rate applied (0.0, 0.10, 0.20)
    public int Quantity { get; set; }

    public BookingItem() { }



    public void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ServiceName;
        yield return ServiceId;
        yield return UnitAdjustedPrice;
        yield return Quantity;
    }
}
