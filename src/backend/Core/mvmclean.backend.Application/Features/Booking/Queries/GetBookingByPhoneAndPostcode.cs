using MediatR;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Booking.Enums;

namespace mvmclean.backend.Application.Features.Booking.Queries;

public class GetBookingByPhoneAndPostcodeRequest : IRequest<GetBookingByPhoneAndPostcodeResponse>
{
    public string PhoneNumber { get; set; } = default!;
    public string Postcode { get; set; } = default!;
}

public class GetBookingByPhoneAndPostcodeResponse
{
    public string Id { get; set; }
    public string PhoneNumber { get; set; }
    public string Postcode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ScheduledSlotStartTime { get; set; }
    public string? ScheduledSlotEndTime { get; set; }
    public string? ServiceAddress { get; set; }
}

public class ServiceItemResponse
{
    public string ServiceName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class AddressResponse
{
    public string Street { get; set; }
    public string City { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class ScheduledSlotResponse
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class GetBookingByPhoneAndPostcodeHandler : IRequestHandler<GetBookingByPhoneAndPostcodeRequest, GetBookingByPhoneAndPostcodeResponse>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingByPhoneAndPostcodeHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<GetBookingByPhoneAndPostcodeResponse> Handle(GetBookingByPhoneAndPostcodeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new ArgumentException("Phone number is required");

        if (string.IsNullOrWhiteSpace(request.Postcode))
            throw new ArgumentException("Postcode is required");

        var allBookings = _bookingRepository.Get(
            predicate: null,
            noTracking: false,
            b => b.ServiceItems,
            b => b.Customer
        ).Where(i=>i.Status == BookingStatus.Confirmed).ToList();        
        
        var booking = allBookings.FirstOrDefault(b =>
            b.PhoneNumber.Value == request.PhoneNumber &&
            b.Postcode.ToString().Replace(" ", "").ToUpper() == request.Postcode.Replace(" ", "").ToUpper());

        if (booking == null)
            throw new KeyNotFoundException($"No booking found for phone {request.PhoneNumber} and postcode {request.Postcode}");

        var response = new GetBookingByPhoneAndPostcodeResponse
        {
            Id = booking.Id.ToString(),
            PhoneNumber = booking.PhoneNumber.Value,
            Postcode = booking.Postcode.Value,
            CustomerName = booking.Customer?.FullName,
            CustomerEmail = booking.Customer?.Email,
            TotalPrice = booking.TotalPrice?.Amount ?? 0m,
            Status = booking.Status.ToString(),
            CreatedAt = booking.CreatedAt,
            ScheduledSlotStartTime = booking.ScheduledSlot?.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
            ScheduledSlotEndTime = booking.ScheduledSlot?.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
            ServiceAddress = booking.ServiceAddress != null 
                ? $"{booking.ServiceAddress.Street}, {booking.ServiceAddress.City}" 
                : null
        };

        return response;
    }
}
