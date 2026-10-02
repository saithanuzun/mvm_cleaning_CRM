using MediatR;
using Microsoft.AspNetCore.Mvc;
using mvmclean.backend.Application.Features.Booking.Queries;

namespace mvmclean.backend.WebApp.Controllers;

public class BookingController : BaseController
{
    private readonly IMediator _mediator;

    public BookingController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpGet]
    [Route("/booking/find")]
    public IActionResult Find()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/booking/find")]
    public async Task<IActionResult> Find(string phoneNumber, string postcode)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(postcode))
        {
            ModelState.AddModelError("", "Please enter both phone number and postcode");
            return View();
        }

        try
        {
            var request = new GetBookingByPhoneAndPostcodeRequest
            {
                PhoneNumber = phoneNumber.Replace(" ",""),
                Postcode = postcode.Replace(" ", "").ToUpper()
            };

            var booking = await _mediator.Send(request);
            
            
            // Redirect to details page with booking ID
            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }
        catch (KeyNotFoundException)
        {
            ModelState.AddModelError("", "No booking found with the provided phone number and postcode");
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            return View();
        }
    }
    
    [HttpGet]
    [Route("/booking/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        try
        {
            var request = new GetBookingByIdRequest { BookingId = id };
            var booking = await _mediator.Send(request);
            return View(booking);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            return View();
        }
    }
}
