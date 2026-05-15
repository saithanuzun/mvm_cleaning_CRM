using MediatR;
using Microsoft.AspNetCore.Mvc;
using mvmclean.backend.Application.Features.Whatsapp.Commands;
using mvmclean.backend.Application.Features.Whatsapp.Models;

namespace mvmclean.backend.WebApp.Areas.Api.Controllers;

[Route("api/[controller]")]
public class WhatsappController : BaseApiController
{
    public WhatsappController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Receives incoming messages from the external WhatsApp API, generates an LLM reply, and optionally sends it back.
    /// </summary>
    [HttpPost("incoming")]
    public async Task<IActionResult> Incoming([FromBody] ExternalWhatsAppIncomingRequest request)
    {
        if (request == null)
            return Error("Invalid request payload");

        try
        {
            var response = await _mediator.Send(new HandleIncomingWhatsAppMessageRequest
            {
                MessageId = request.MessageId,
                From = request.From,
                ContactName = request.ContactName,
                Message = request.Message,
                Timestamp = request.Timestamp,
                ConversationId = request.ConversationId
            });

            if (!response.Success)
                return Error(response.Message, 500);

            return Success(new ExternalWhatsAppIncomingReply
            {
                Reply = response.Reply ?? string.Empty,
                MessageId = response.MessageId,
                SentViaExternalApi = response.SentViaExternalApi
            }, response.Message);
        }
        catch (Exception ex)
        {
            return Error($"Error processing incoming WhatsApp message: {ex.Message}", 500);
        }
    }

    /// <summary>
    /// Sends a WhatsApp message via the external WhatsApp API (manual / admin use).
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendWhatsAppMessageRequest request)
    {
        if (!ModelState.IsValid)
            return Error("Invalid request data");

        try
        {
            var response = await _mediator.Send(request);

            if (!response.Success)
                return Error(response.Message);

            return Success(new
            {
                providerMessageId = response.ProviderMessageId
            }, response.Message);
        }
        catch (Exception ex)
        {
            return Error($"Error sending WhatsApp message: {ex.Message}", 500);
        }
    }
}
