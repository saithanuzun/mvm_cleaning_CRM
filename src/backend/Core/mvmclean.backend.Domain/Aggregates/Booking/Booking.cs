using mvmclean.backend.Domain.Aggregates.Booking.Entities;
using mvmclean.backend.Domain.Aggregates.Booking.Enums;
using mvmclean.backend.Domain.Aggregates.Booking.Events;
using mvmclean.backend.Domain.Aggregates.Service; // Add reference to Service aggregate
using mvmclean.backend.Domain.SharedKernel.ValueObjects;
using System.Collections.ObjectModel;
using mvmclean.backend.Domain.Aggregates.Booking.ValueObjects;

namespace mvmclean.backend.Domain.Aggregates.Booking;

public class Booking : Core.BaseClasses.AggregateRoot
{
    //first step to create booking required field
    public PhoneNumber PhoneNumber { get; set; }
    public Postcode Postcode { get; set; }

    //second step assign contractor
    public Guid? ContractorId { get; private set; }
    public Contractor.Contractor? Contractor { get; private set; }

    //third step add booking items
    private readonly List<BookingItem> _serviceItems = new();
    public List<BookingItem> ServiceItems => new List<BookingItem>(_serviceItems);

    // total price is here
    public Money? TotalPrice { get; private set; }

    //time slot
    public TimeSlot? ScheduledSlot { get; private set; }


    // customer details and address
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public Address? ServiceAddress { get; private set; }

    // payment
    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }

    // lastly booking status

    public BookingCreationStatus CreationStatus { get; private set; }
    public BookingStatus Status { get; private set; }


    private Booking(PhoneNumber number, Postcode postcode)
    {
        PhoneNumber = number;
        Postcode = postcode;
        CreationStatus = BookingCreationStatus.BasicInfo;
    }

    protected Booking()
    {
    }

    public static Booking Create(Postcode postcode, PhoneNumber phoneNumber)
    {
        var booking = new Booking(phoneNumber, postcode);

        booking.AddDomainEvent(new BookingCreatedEvent(booking.Id, phoneNumber, postcode));
        return booking;
    }

    public void SelectContractor(Contractor.Contractor contractor)
    {
        if (contractor == null)
            throw new ArgumentNullException(nameof(contractor));

        if (!contractor.CoversPostcode(Postcode))
        {
            throw new InvalidOperationException("Contractor is not available for this postcode");
        }


        ContractorId = contractor.Id;
        CreationStatus = BookingCreationStatus.ContractorSelected;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ContractorAssignedEvent(Id, contractor.Id));
    }

    public void AddServiceToCart(string name, Guid serviceItemId, Money unitAdjustedPrice, int quantity = 1)
    {
        var existingItem = _serviceItems.FirstOrDefault(s => s.ServiceId == serviceItemId);

        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            // Apply tiered discount based on number of distinct services already in cart
            // 1st service: 0% discount (full price)
            // 2nd service: 10% discount
            // 3rd+ services: 20% discount
            int serviceCount = _serviceItems.Count;
            decimal discountRate = serviceCount switch
            {
                0 => 0m,      // 1st service - full price
                1 => 0.10m,   // 2nd service - 10% off
                _ => 0.20m    // 3rd+ services - 20% off
            };

            var discountedPrice = unitAdjustedPrice * (1m - discountRate);

            var bookingItem = new BookingItem
            {
                ServiceName = name,
                ServiceId = serviceItemId,
                UnitAdjustedPrice = discountedPrice,
                OriginalPrice = unitAdjustedPrice,
                DiscountRate = discountRate,
                Quantity = quantity,
            };
            _serviceItems.Add(bookingItem);

            //AddDomainEvent(new ServiceAddedToCartEvent(serviceItemId, unitAdjustedPrice, quantity));
        }

        UpdateTotalPrice();
        UpdatedAt = DateTime.UtcNow;
    }

    // Remove service from cart
    public void RemoveServiceFromCart(Guid serviceItemId)
    {
        var item = _serviceItems.FirstOrDefault(s => s.ServiceId == serviceItemId);
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.UpdateQuantity(item.Quantity -= 1);
            }
            else
            {
                _serviceItems.Remove(item);
            }
            UpdateTotalPrice();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    private void UpdateTotalPrice()
    {
        TotalPrice = Money.Zero();

        foreach (var item in _serviceItems)
        {
            TotalPrice += item.UnitAdjustedPrice * item.Quantity;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTimeSlot(TimeSlot timeSlot, Contractor.Contractor contractor)
    {
        if (!contractor.IsAvailableAt(timeSlot))
            throw new InvalidOperationException("TimeSlot is not available for this time slot");

        ScheduledSlot = timeSlot;
        CreationStatus = BookingCreationStatus.TimeSlotSelected;

        UpdatedAt = DateTime.UtcNow;
    }
    public void AssignServiceAddress(Address address)
    {
        ServiceAddress = address;
    }

    public void AssignCustomer(Email? email, string? FirstName, string? LastName, string? street, string? city, string? additionalInfo)
    {
        var address = Address.Create(street, city, Postcode, additionalInfo);

        var customer = Customer.Create(PhoneNumber, email: email, FirstName, LastName, address);

        Customer = customer;
        CustomerId = customer.Id;
        ServiceAddress = customer.Address; //this can be different address as well, I just didnt want to implement now //Todo
        CreationStatus = BookingCreationStatus.CustomerAdded;

        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignPayment(Payment payment)
    {
        CreationStatus = BookingCreationStatus.PaymentInitiated;

        PaymentId = payment.Id;
        Payment = payment;
        UpdatedAt = DateTime.UtcNow;
    }


    public void ApplyPromotion(Promotion.Promotion promotion)
    {
        TotalPrice = promotion.ApplyDiscount(TotalPrice);
    }


    public void UpdateServiceAddress(Address newAddress)
    {
        ServiceAddress = newAddress ?? throw new ArgumentNullException(nameof(newAddress));

        UpdatedAt = DateTime.UtcNow;

        //AddDomainEvent(new ServiceAddressUpdatedEvent(Id, newAddress));
    }


    #region BookingFlow

    public void Confirm()
    {
        if (ContractorId == null)
            throw new InvalidOperationException("Cannot confirm booking without assigned contractor");

        if (_serviceItems.Count == 0)
            throw new InvalidOperationException("Cannot confirm booking without services");

        Status = BookingStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new BookingConfirmedEvent(Id, ContractorId, TotalPrice, ScheduledSlot, ServiceItems, Customer.FullName, ServiceAddress));
    }

    public void Start()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed bookings can be started");

        Status = BookingStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = BookingStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string? cancellationReason = null)
    {
        if (Status == BookingStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed bookings");

        var previousStatus = Status;
        Status = BookingStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion
}