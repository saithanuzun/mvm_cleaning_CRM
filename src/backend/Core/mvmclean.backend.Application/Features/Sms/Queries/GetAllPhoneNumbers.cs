using Microsoft.Extensions.Configuration;
using MediatR;
using mvmclean.backend.Domain.Aggregates.Booking;

namespace mvmclean.backend.Application.Features.Sms.Queries;

public class GetAllPhoneNumbersRequest : IRequest<GetAllPhoneNumbersResponse>
{
}

public class GetAllPhoneNumbersResponse
{
    public List<string> PhoneNumbers { get; set; } = new();
    public int TotalCount { get; set; }
    public int FromDatabase { get; set; }
    public int FromTextFile { get; set; }
}

public class GetAllPhoneNumbersHandler : IRequestHandler<GetAllPhoneNumbersRequest, GetAllPhoneNumbersResponse>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IConfiguration _configuration;
    private readonly string _phoneNumbersFilePath;

    public GetAllPhoneNumbersHandler(IBookingRepository bookingRepository, IConfiguration configuration)
    {
        _bookingRepository = bookingRepository;
        _configuration = configuration;

        // Get path from configuration or use default
        var configPath = _configuration["SmsSettings:PhoneNumbersFilePath"] ?? "Data/phoneNumbers.txt";

        if (Path.IsPathRooted(configPath))
        {
            _phoneNumbersFilePath = configPath;
        }
        else
        {
            // Try content root (for production/deployment)
            var contentRoot = Directory.GetCurrentDirectory();
            var pathCheck = Path.Combine(contentRoot, configPath);

            if (File.Exists(pathCheck))
            {
                _phoneNumbersFilePath = pathCheck;
            }
            else
            {
                // Fallback for local development (looking for Infrastructure project)
                _phoneNumbersFilePath = Path.Combine(
                    contentRoot,
                    "..", "..", "Infrastructure", "mvmclean.backend.Infrastructure", "Data", "phoneNumbers.txt"
                );
            }
        }
    }

    public async Task<GetAllPhoneNumbersResponse> Handle(GetAllPhoneNumbersRequest request, CancellationToken cancellationToken)
    {
        var phoneNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int dbCount = 0;
        int fileCount = 0;

        // Get phone numbers from database (Booking table)
        var bookings = _bookingRepository.Get(
            predicate: null!,
            noTracking: true
        ).ToList();

        foreach (var booking in bookings)
        {
            if (booking.PhoneNumber?.Value != null && !string.IsNullOrWhiteSpace(booking.PhoneNumber.Value))
            {
                var cleanNumber = CleanPhoneNumber(booking.PhoneNumber.Value);
                if (!string.IsNullOrWhiteSpace(cleanNumber))
                {
                    phoneNumbers.Add(cleanNumber);
                    dbCount++;
                }
            }
        }

        // Get phone numbers from text file
        if (File.Exists(_phoneNumbersFilePath))
        {
            var lines = await File.ReadAllLinesAsync(_phoneNumbersFilePath, cancellationToken);
            foreach (var line in lines)
            {
                var cleanNumber = CleanPhoneNumber(line);
                if (!string.IsNullOrWhiteSpace(cleanNumber))
                {
                    phoneNumbers.Add(cleanNumber);
                    fileCount++;
                }
            }
        }

        return new GetAllPhoneNumbersResponse
        {
            PhoneNumbers = phoneNumbers.OrderBy(p => p).ToList(),
            TotalCount = phoneNumbers.Count,
            FromDatabase = dbCount,
            FromTextFile = fileCount
        };
    }

    private string CleanPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return string.Empty;

        // Remove common formatting characters but keep the number
        return phoneNumber.Trim();
    }
}
