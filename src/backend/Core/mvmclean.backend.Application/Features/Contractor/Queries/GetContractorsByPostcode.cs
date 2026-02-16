using MediatR;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Contractor;
using mvmclean.backend.Domain.SharedKernel.ValueObjects;

namespace mvmclean.backend.Application.Features.Contractor.Queries;

public class GetContractorsByPostcodeRequest : IRequest<GetContractorsByPostcodeResponse>
{
    public string Postcode { get; set; }
    public string BookingId { get; set; }
}

public class GetContractorsByPostcodeResponse
{
    public List<string> ContractorIds { get; set; }
    public string BookingId { get; set; }
}

public class GetContractorByPostcodeHandler : IRequestHandler<GetContractorsByPostcodeRequest, GetContractorsByPostcodeResponse>
{
    private IContractorRepository _contractorRepository;


    public GetContractorByPostcodeHandler(IBookingRepository bookingRepository, IContractorRepository contractorRepository)
    {
        _contractorRepository = contractorRepository;
    }


    public async Task<GetContractorsByPostcodeResponse> Handle(GetContractorsByPostcodeRequest request, CancellationToken cancellationToken)
    {
        //var booking = await _bookingRepository.GetByIdAsync(Guid.Parse(request.BookingId));

        var targetPostcode = Postcode.Create(request.Postcode);

        // Fetch only active contractors with their coverage areas
        var activeContractors = await _contractorRepository.GetList(
            predicate: c => c.IsActive,
            noTracking: true,
            includes: i => i.CoverageAreas);

        var contractors = activeContractors
            .Where(c => c.CoversPostcode(targetPostcode))
            .OrderBy(c => c.BookedCount)
            .ToList();

        var contractorIds = contractors.Select(c => c.Id.ToString()).ToList();

        return new GetContractorsByPostcodeResponse
        {
            BookingId = request.BookingId,
            ContractorIds = contractorIds,
        };
    }
}