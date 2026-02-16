using MediatR;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Booking.Enums;
using mvmclean.backend.Domain.Aggregates.Contractor;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;
using mvmclean.backend.Application.Features.Contractor.Queries;

namespace mvmclean.backend.Application.Features.Booking.Queries;

public class GetAvailableSlotsRequest : IRequest<GetAvailableSlotsResponse>
{
    public string Postcode { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? ContractorIds { get; set; }
}

public class GetAvailableSlotsResponse
{
    public List<AvailableSlotDto> AvailableSlots { get; set; } = new();
    public string Message { get; set; }
}

public class AvailableSlotDto
{
    public string SlotId { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string ContractorId { get; set; }
    public string ContractorName { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class GetAvailableSlotsHandler : IRequestHandler<GetAvailableSlotsRequest, GetAvailableSlotsResponse>
{
    private readonly IContractorRepository _contractorRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMediator _mediator;

    public GetAvailableSlotsHandler(
        IContractorRepository contractorRepository,
        IBookingRepository bookingRepository,
        IMediator mediator)
    {
        _contractorRepository = contractorRepository;
        _bookingRepository = bookingRepository;
        _mediator = mediator;
    }

    public async Task<GetAvailableSlotsResponse> Handle(GetAvailableSlotsRequest request, CancellationToken cancellationToken)
    {
        if (request.Date < DateTime.Today)
        {
            return new GetAvailableSlotsResponse
            {
                AvailableSlots = new List<AvailableSlotDto>(),
                Message = "Cannot book for past dates"
            };
        }

        // Determine contractor IDs based on postcode or explicit IDs
        List<string> contractorIdList;

        if (!string.IsNullOrEmpty(request.ContractorIds))
        {
            contractorIdList = request.ContractorIds
                .Split(',')
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();
        }
        else if (!string.IsNullOrEmpty(request.Postcode))
        {
            var postcodeRequest = new GetContractorsByPostcodeRequest
            {
                Postcode = request.Postcode,
                BookingId = ""
            };

            var contractorsResponse = await _mediator.Send(postcodeRequest, cancellationToken);
            contractorIdList = contractorsResponse.ContractorIds;
        }
        else
        {
            return new GetAvailableSlotsResponse { AvailableSlots = new List<AvailableSlotDto>(), Message = "No postcode or contractors provided" };
        }

        if (!contractorIdList.Any())
        {
            return new GetAvailableSlotsResponse
            {
                AvailableSlots = new List<AvailableSlotDto>(),
                Message = "No contractors available for this area"
            };
        }

        // Get availability for the day
        var availabilityRequest = new GetContractorAvailabilityByDayRequest
        {
            ContractorIds = contractorIdList,
            Date = request.Date,
            Duration = TimeSpan.FromMinutes(request.DurationMinutes)
        };

        var availabilityResponse = await _mediator.Send(availabilityRequest, cancellationToken);

        if (availabilityResponse == null || availabilityResponse.Count == 0)
            return new GetAvailableSlotsResponse { AvailableSlots = new List<AvailableSlotDto>(), Message = "No available slots" };

        // Bulk fetch contractor names
        var contractorGuids = contractorIdList
            .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();

        var contractors = await _contractorRepository.GetList(
            predicate: c => contractorGuids.Contains(c.Id),
            noTracking: true);

        var contractorMap = contractors.ToDictionary(c => c.Id.ToString(), c => c.FullName);

        // Format response
        var slots = new List<AvailableSlotDto>();
        var slotIndex = 0;

        var groupedByContractor = availabilityResponse
            .GroupBy(r => r.ContractorId)
            .ToList();

        foreach (var contractorGroup in groupedByContractor)
        {
            var contractorName = contractorMap.ContainsKey(contractorGroup.Key)
                ? contractorMap[contractorGroup.Key]
                : "Unknown";

            var availableSlots = contractorGroup.Where(s => s.Available).ToList();

            foreach (var slot in availableSlots)
            {
                slots.Add(new AvailableSlotDto
                {
                    SlotId = $"{contractorGroup.Key}_{slotIndex}",
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    ContractorId = contractorGroup.Key,
                    ContractorName = contractorName,
                    IsAvailable = true
                });
                slotIndex++;
            }
        }

        return new GetAvailableSlotsResponse
        {
            AvailableSlots = slots.OrderBy(s => s.StartTime).ToList(),
            Message = slots.Any() ? "Slots found" : "No available slots"
        };
    }
}
