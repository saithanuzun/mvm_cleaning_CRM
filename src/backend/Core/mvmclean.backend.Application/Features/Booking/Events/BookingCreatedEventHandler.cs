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

            await SendBookingWelcomeWhatsAppAsync(notification, cancellationToken);

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
            ?? "Welcome to MvM Cleaning Shop! I'm Emma, your WhatsApp assistant. Ask me anything about our services — start your message with \"emma\" and I'll help you.";

        try
        {
            var result = await _whatsAppService.SendMessageAsync(
                notification.PhoneNumber.Value,
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
