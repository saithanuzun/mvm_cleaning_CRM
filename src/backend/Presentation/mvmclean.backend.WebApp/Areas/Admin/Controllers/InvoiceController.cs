using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mvmclean.backend.Infrastructure.InvoicingService;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace mvmclean.backend.WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/invoice")]
[Authorize(AuthenticationSchemes = "AdminCookie")]
public class InvoiceController : BaseAdminController
{
    private readonly InvoiceCreator _invoiceCreator;

    public InvoiceController(IMediator mediator, InvoiceCreator invoiceCreator) : base(mediator)
    {
        _invoiceCreator = invoiceCreator;
    }

    [Route("")]
    [Route("index")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateInvoiceViewModel
        {
            DateTime = DateTime.Now
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate([FromForm] CreateInvoiceViewModel model)
    {
        try
        {
            // Generate payment due text
            string paymentDueText = model.PaymentDue switch
            {
                -1 => "Due on Receipt",
                0 => "Paid",
                _ => $"Due in {model.PaymentDue} days"
            };

            // Generate HTML invoice
            var html = _invoiceCreator.CreateInvoiceHtml(
                model.Amount,
                model.ClientAddress,
                model.ClientName,
                model.DateTime,
                model.Description,
                paymentDueText
            );

            // Convert HTML to PDF using PuppeteerSharp
            byte[] pdfBytes = await ConvertHtmlToPdfWithPuppeteer(html);
        
            // Generate filename
            string fileName = $"Invoice_{model.ClientName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        
            // Return PDF for download
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error generating invoice: {ex.Message}";
            return View("Create", model);
        }
    }

    private async Task<byte[]> ConvertHtmlToPdfWithPuppeteer(string html)
    {
        // Download browser if not already downloaded
        await new BrowserFetcher().DownloadAsync();

        using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true }))
        using (var page = await browser.NewPageAsync())
        {
            // Set HTML content
            await page.SetContentAsync(html);

            // Generate PDF
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                
            });

            return pdfBytes;
        }
    }
    
}

public class CreateInvoiceViewModel
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientAddress { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentDue { get; set; } = -1;
    public DateTime DateTime { get; set; } = DateTime.Now;
}
