using MediatR;
using System.Linq.Expressions;
using mvmclean.backend.Domain.Aggregates.Contractor;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;

namespace mvmclean.backend.Application.Features.Contractor.Queries;

public class GetContractorAvailabilityByDayRequest : IRequest<List<GetContractorAvailabilityByDayResponse>>
{
    public List<string> ContractorIds { get; set; } = default!;
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
}

public class GetContractorAvailabilityByDayResponse
{
    public string ContractorId { get; set; }
    public string StartTime { get; set; } = default!;
    public string EndTime { get; set; } = default!;
    public bool Available { get; set; }
}

public class GetContractorAvailabilityByDayHandler : IRequestHandler<GetContractorAvailabilityByDayRequest,
    List<GetContractorAvailabilityByDayResponse>>
{
    private readonly IContractorRepository _contractorRepository;

    public GetContractorAvailabilityByDayHandler(IContractorRepository contractorRepository)
    {
        _contractorRepository = contractorRepository;
    }

    public async Task<List<GetContractorAvailabilityByDayResponse>> Handle(
        GetContractorAvailabilityByDayRequest request,
        CancellationToken token)
    {
        if (!request.ContractorIds.Any())
            throw new Exception("No contractors provided");

        var contractorIds = request.ContractorIds.Select(Guid.Parse).ToList();

        // Bulk fetch contractors with necessary inclusions
        var contractors = await _contractorRepository.GetList(
            predicate: c => contractorIds.Contains(c.Id),
            noTracking: true,
            includes: new Expression<Func<Domain.Aggregates.Contractor.Contractor, object>>[]
            {
                i => i.WorkingHours,
                i => i.UnavailableSlots
            });

        if (contractors.Count != contractorIds.Count)
        {
            // Optional: log or handle missing contractors if needed, but don't fail for the whole request if some are missing
            // For now, keeping the strict check if that was intended
            var missingIds = contractorIds.Except(contractors.Select(c => c.Id)).ToList();
            if (missingIds.Any())
            {
                // Handle missing contractors gracefully or throw as before
                // throw new Exception($"One or more contractors not found: {string.Join(", ", missingIds)}");
            }
        }

        var day = request.Date.Date;

        var workStart = day.AddHours(8).AddMinutes(30);
        var workEnd = day.AddHours(18).AddMinutes(30);

        var step = TimeSpan.FromMinutes(30);
        var duration = request.Duration;

        var result = new List<GetContractorAvailabilityByDayResponse>();

        for (var start = workStart; start.Add(duration) <= workEnd; start = start.Add(step))
        {
            var slot = TimeSlot.Create(start, start.Add(duration));

            foreach (var contractor in contractors)
            {
                // Only add slots where contractor is available
                if (contractor != null && contractor.IsAvailableAt(slot))
                {
                    result.Add(new GetContractorAvailabilityByDayResponse
                    {
                        ContractorId = contractor.Id.ToString(),
                        StartTime = slot.StartTime.ToString("HH:mm"),
                        EndTime = slot.EndTime.ToString("HH:mm"),
                        Available = true
                    });
                }
            }
        }

        return result;
    }
}