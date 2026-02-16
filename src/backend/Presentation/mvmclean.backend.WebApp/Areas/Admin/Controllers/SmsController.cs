using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mvmclean.backend.Application.Features.Sms.Commands;
using mvmclean.backend.Application.Features.Sms.Queries;

namespace mvmclean.backend.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Sms")]
[Authorize(AuthenticationSchemes = "AdminCookie")]
public class SmsController : BaseAdminController
{
    public SmsController(IMediator mediator) : base(mediator)
    {
    }

    [Route("")]
    [Route("index")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _mediator.Send(new GetAllPhoneNumbersRequest());
            return View(response);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error loading phone numbers: {ex.Message}";
            return View(new GetAllPhoneNumbersResponse());
        }
    }

    [Route("send")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendBulkSms([FromForm] List<string> phoneNumbers, [FromForm] string message)
    {
        try
        {
            if (phoneNumbers == null || !phoneNumbers.Any())
            {
                TempData["Error"] = "Please select at least one phone number";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please enter a message";
                return RedirectToAction(nameof(Index));
            }

            var response = await _mediator.Send(new SendBulkSmsRequest
            {
                PhoneNumbers = phoneNumbers,
                Message = message
            });

            if (response.Success)
            {
                TempData["Success"] = $"SMS sent successfully to {response.SuccessCount} numbers";
            }
            else
            {
                TempData["Warning"] = response.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error sending SMS: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
