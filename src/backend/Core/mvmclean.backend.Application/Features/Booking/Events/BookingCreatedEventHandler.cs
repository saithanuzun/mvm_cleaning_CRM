using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mvmclean.backend.Application.Services;
using mvmclean.backend.Domain.Aggregates.Booking.Events;

namespace mvmclean.backend.Application.Features.Booking.Events;

public class BookingCreatedEventHandler : INotificationHandler<BookingCreatedEvent>
{
    private readonly IMailingService _mailingService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BookingCreatedEventHandler> _logger;

    public BookingCreatedEventHandler(
        IMailingService mailingService,
        IWhatsAppService whatsAppService,
        IConfiguration configuration,
        ILogger<BookingCreatedEventHandler> logger)
    {
        _mailingService = mailingService ?? throw new ArgumentNullException(nameof(mailingService));
        _whatsAppService = whatsAppService ?? throw new ArgumentNullException(nameof(whatsAppService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(BookingCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _mailingService.SendBookingCreatedNotificationAsync(
                recipientEmail: "saithan.uzun@gmail.com", // TODO: get email form somwehere else
                bookingId: notification.BookingId,
                postcode: notification.Postcode.Value,
                telephoneNumber: notification.PhoneNumber.Value
            );

            // TODO: enable whatsapp
            //await SendBookingWelcomeWhatsAppAsync(notification, cancellationToken);

            _logger.LogInformation(
                "Booking created event handled: BookingId {BookingId}, Postcode {Postcode}, Phone {Phone}",
                notification.BookingId,
                notification.Postcode.Value,
                notification.PhoneNumber.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling BookingCreatedEvent for booking {BookingId}", notification.BookingId);
            throw;
        }
    }

    private async Task SendBookingWelcomeWhatsAppAsync(
        BookingCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        var welcomeMessage = _configuration["WhatsApp:BookingWelcomeMessage"]
            ?? "Welcome to MvM Cleaning Shop! I'm Emma, your WhatsApp assistant. Ask me anything about our services — start your message with \"emma\" and I'll help you. ->" +
             notification.Postcode;

        try
        {
            var number = PhoneNormalizer.ToUkInternational(notification.PhoneNumber.Value);
            var result = await _whatsAppService.SendMessageAsync(
                number,
                welcomeMessage,
                cancellationToken);

            if (!result.Success)
            {
                _logger.LogWarning(
                    "Booking welcome WhatsApp failed for {Phone}: {Message}",
                    notification.PhoneNumber.Value,
                    result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Booking welcome WhatsApp could not be sent to {Phone}",
                notification.PhoneNumber.Value);
        }
    }
    
    
}



public static class PhoneNormalizer
{
    public static string ToUkInternational(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        // Remove everything except digits
        string digits = Regex.Replace(input, @"\D", "");

        if (digits.Length == 0)
            return null;

        // Case 1: already starts with 44
        if (digits.StartsWith("44"))
        {
            return digits;
        }

        // Case 2: starts with 0 (UK local format)
        if (digits.StartsWith("0"))
        {
            digits = digits.Substring(1);
            return "44" + digits;
        }

        // Case 3: missing 0 but is UK local number (10 digits typical mobile)
        // e.g. 7862254412 → assume UK mobile
        if (digits.Length == 10)
        {
            return "44" + digits;
        }

        // Fallback: just prepend 44 if it looks like UK number
        return "44" + digits;
    }
}