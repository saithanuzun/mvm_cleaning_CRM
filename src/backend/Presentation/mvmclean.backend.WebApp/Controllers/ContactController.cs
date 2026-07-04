using MediatR;
using Microsoft.AspNetCore.Mvc;
using mvmclean.backend.Application.Features.Contact.Commands;

namespace mvmclean.backend.WebApp.Controllers;

public class ContactController : BaseController
{
    private readonly IMediator _mediator;

    public ContactController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Route("/contact")]
    public IActionResult Index() => View();

    [HttpPost]
    [Route("/contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitForm(
        [FromForm] string? fullName,
        [FromForm] string? email,
        [FromForm] string? phoneNumber,
        [FromForm] string? subject,
        [FromForm] string? message,
        [FromForm] string? website
        )
    {
        if (!string.IsNullOrEmpty(website) || email == "zekisuquc419@gmail.com" || email == "charmaynesaunders62@gmail.com") // todo: create blacklist in database
        {
            return BadRequest();
        }

        // Validate required fields: email and message
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            Error("Please provide at least an email and message");
            return View(nameof(Index));
        }

        // Create contact command with form data
        var command = new CreateContactCommand
        {
            FullName = fullName ?? "Anonymous",
            Email = email,
            PhoneNumber = phoneNumber,
            Subject = subject ?? "Contact Form Submission",
            Message = message
        };

        var result = await _mediator.Send(command);

        if (result.Success)
        {
            Success("Thank you for contacting us! We will respond to your inquiry shortly.");
            return RedirectToAction(nameof(Index));
        }

        Error(result.Message ?? "An error occurred while processing your request");
        return View(nameof(Index));
    }
}
