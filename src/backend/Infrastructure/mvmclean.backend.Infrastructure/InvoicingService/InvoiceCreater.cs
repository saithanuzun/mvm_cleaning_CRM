namespace mvmclean.backend.Infrastructure.InvoicingService;

using System.Text;

public class InvoiceCreator
{
    /// <summary>
    /// Generates a professional HTML invoice string.
    /// </summary>
    /// <param name="amount">The total cost of the service.</param>
    /// <param name="address">The client's physical address.</param>
    /// <param name="name">The client's name.</param>
    /// <returns>A string containing the full HTML of the invoice.</returns>
public string CreateInvoiceHtml(decimal amount, string address, string name, DateTime dateTime, string description, string paymentDue)
{
    // 1. Create a unique Invoice ID
    string invoiceId = "INV-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    
    // Format date
    string invoiceDate = dateTime.ToString("dd MMMM yyyy");
    
    // 2. Build the HTML
    StringBuilder html = new StringBuilder();

    html.Append($@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Invoice {invoiceId} - MVM Cleaning</title>
    <style>
        /* Base Styles */
        * {{
            box-sizing: border-box;
            margin: 0;
            padding: 0;
        }}
        
        body {{
            font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif;
            color: #333;
            margin: 0;
            padding: 20px;
            background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
            min-height: 100vh;
        }}
        
        .invoice-container {{
            max-width: 900px;
            margin: 0 auto;
        }}
        
        .invoice-box {{
            background: white;
            border-radius: 16px;
            box-shadow: 0 10px 40px rgba(0, 0, 0, 0.08),
                        0 2px 10px rgba(0, 0, 0, 0.04);
            overflow: hidden;
            position: relative;
        }}
        
        /* Decorative Elements */
        .invoice-box::before {{
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, #103153 0%, #5BB5E6 50%, #F49E73 100%);
        }}
        
        /* Header Styles */
        .header {{
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            padding: 40px 40px 20px;
            background: linear-gradient(135deg, #103153 0%, #1a4478 100%);
            color: white;
            position: relative;
        }}
        
        .header::after {{
            content: '';
            position: absolute;
            bottom: 0;
            left: 5%;
            right: 5%;
            height: 1px;
            background: rgba(255, 255, 255, 0.2);
        }}
        
        .logo-area {{
            flex: 1;
            display: flex;
            align-items: center;
        }}
        
        .logo-area img {{
            max-width: 180px;
            height: auto;
            transition: transform 0.3s ease;
            padding: 10px;
            border-radius: 4px;
        }}
        
        .logo-area img:hover {{
            transform: scale(1.02);
        }}
        
        .company-info {{
            text-align: right;
            flex: 1;
        }}
        
        .company-info h2 {{
            margin: 0 0 10px 0;
            font-size: 28px;
            font-weight: 700;
            letter-spacing: 1px;
            color: white;
        }}
        
        .company-info .tagline {{
            font-size: 14px;
            opacity: 0.9;
            margin-bottom: 8px;
        }}
        
        .company-info .contact {{
            font-size: 13px;
            opacity: 0.8;
            line-height: 1.5;
        }}
        
        /* Invoice Meta Section */
        .invoice-meta {{
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 30px;
            padding: 30px 40px;
            background: #f8fafc;
        }}
        
        .invoice-badge {{
            background: linear-gradient(135deg, #5BB5E6, #4a9fd6);
            color: white;
            padding: 8px 20px;
            border-radius: 20px;
            font-size: 14px;
            font-weight: 600;
            display: inline-block;
            margin-bottom: 15px;
            box-shadow: 0 4px 12px rgba(91, 181, 230, 0.3);
        }}
        
        .bill-to h3 {{
            color: #103153;
            margin-bottom: 15px;
            font-size: 18px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }}
        
        .bill-to h3::before {{
            content: '👤';
            font-size: 16px;
        }}
        
        .bill-to p {{
            line-height: 1.6;
            background: white;
            padding: 15px;
            border-radius: 8px;
            border-left: 4px solid #F49E73;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
        }}
        
        .invoice-details {{
            background: white;
            padding: 15px;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
        }}
        
        .invoice-details h3 {{
            color: #103153;
            margin-bottom: 15px;
            font-size: 18px;
            font-weight: 600;
            display: flex;
            align-items: center;
            gap: 10px;
        }}
        
        .invoice-details h3::before {{
            content: '📋';
            font-size: 16px;
        }}
        
        .detail-item {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 8px;
            padding-bottom: 8px;
            border-bottom: 1px solid #f0f0f0;
        }}
        
        .detail-item:last-child {{
            border-bottom: none;
        }}
        
        .detail-label {{
            color: #666;
            font-weight: 500;
        }}
        
        .detail-value {{
            color: #103153;
            font-weight: 600;
        }}
        
        /* Table Styles */
        .table-container {{
            padding: 0 40px 30px;
        }}
        
        table {{
            width: 100%;
            border-collapse: separate;
            border-spacing: 0;
            background: white;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
        }}
        
        thead {{
            background: linear-gradient(135deg, #103153, #1a4478);
        }}
        
        th {{
            color: white;
            padding: 18px 20px;
            font-weight: 600;
            text-align: left;
            font-size: 15px;
        }}
        
        th:first-child {{
            border-radius: 10px 0 0 0;
        }}
        
        th:last-child {{
            border-radius: 0 10px 0 0;
        }}
        
        td {{
            padding: 18px 20px;
            border-bottom: 1px solid #f0f0f0;
            transition: background-color 0.2s ease;
        }}
        
        tr:hover td {{
            background-color: #f8fafc;
        }}
        
        .service-description {{
            color: #103153;
            font-weight: 600;
            font-size: 16px;
        }}
        
        .service-details {{
            color: #666;
            font-size: 14px;
            margin-top: 4px;
            line-height: 1.5;
        }}
        
        /* Summary Section */
        .summary {{
            padding: 25px 40px;
            background: #f8fafc;
            border-top: 1px solid #e8e8e8;
        }}
        
        .summary-row {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 15px;
            font-size: 16px;
        }}
        
        .summary-label {{
            color: #666;
        }}
        
        .summary-value {{
            color: #103153;
            font-weight: 500;
        }}
        
        .total-row {{
            border-top: 2px solid #e8e8e8;
            padding-top: 20px;
            margin-top: 10px;
            font-size: 20px;
            font-weight: 700;
        }}
        
        .total-row .summary-label {{
            color: #103153;
        }}
        
        .total-row .summary-value {{
            color: #103153;
            font-size: 24px;
        }}
        
        /* Footer */
        .footer {{
            padding: 30px 40px;
            background: #103153;
            color: rgba(255, 255, 255, 0.85);
            text-align: center;
        }}
        
        .footer-content {{
            max-width: 600px;
            margin: 0 auto;
        }}
        
        .footer h3 {{
            color: white;
            margin-bottom: 15px;
            font-size: 18px;
        }}
        
        .footer p {{
            font-size: 14px;
            line-height: 1.6;
            margin-bottom: 20px;
            opacity: 0.9;
        }}
        
        .footer .thank-you {{
            font-size: 16px;
            color: #5BB5E6;
            font-weight: 600;
            margin-top: 20px;
            padding-top: 20px;
            border-top: 1px solid rgba(255, 255, 255, 0.1);
        }}
        
        /* Responsive Design */
        @media (max-width: 768px) {{
            .header {{
                flex-direction: column;
                text-align: center;
                padding: 30px 20px;
            }}
            
            .company-info {{
                text-align: center;
                margin-top: 20px;
            }}
            
            .invoice-meta {{
                grid-template-columns: 1fr;
                padding: 20px;
                gap: 20px;
            }}
            
            .table-container,
            .summary {{
                padding: 20px;
            }}
            
            th, td {{
                padding: 12px 15px;
            }}
        }}
        
        @media print {{
            body {{
                background: white !important;
                padding: 0 !important;
            }}
            
            .invoice-box {{
                box-shadow: none !important;
                border-radius: 0 !important;
            }}
        }}
    </style>
</head>
<body>
    <div class='invoice-container'>
        <div class='invoice-box'>
            <!-- Header -->
            <div class='header'>
                <div class='logo-area'>
                    <img src='https://www.mvmcleaning.com/img/Logopng.png' alt='MVM Cleaning Logo'>
                </div>
                <div class='company-info'>
                    <h2>MVM CLEANING</h2>
                    <p class='tagline'>Professional Cleaning Services</p>
                    <p class='contact'>
                        72 Darien Way<br>
                        Leicester LE3 3TT<br>
                        📞 07405392635 | ✉️ Info@mvmcleaning.com<br>
                        🌐 mvmcleaning.com
                    </p>
                </div>
            </div>

            <!-- Invoice Meta Information -->
            <div class='invoice-meta'>
                <div class='bill-to'>
                    <h3>Bill To</h3>
                    <p>
                        <strong>{EscapeHtml(name)}</strong><br>
                        {EscapeHtml(address).Replace("\n", "<br>")}
                    </p>
                </div>
                <div class='invoice-details'>
                    <h3>Invoice Details</h3>
                    <div class='detail-item'>
                        <span class='detail-label'>Invoice #</span>
                        <span class='detail-value'>{invoiceId}</span>
                    </div>
                    <div class='detail-item'>
                        <span class='detail-label'>Invoice Date</span>
                        <span class='detail-value'>{invoiceDate}</span>
                    </div>
                    <div class='detail-item'>
                        <span class='detail-label'>Status</span>
                        <span class='detail-value' style='color: #F49E73; font-weight: 600;'>
                             {paymentDue}
                        </span>
                    </div>
                </div>
            </div>

            <!-- Service Table -->
            <div class='table-container'>
                <table>
                    <thead>
                        <tr>
                            <th style='width: 60%;'>Service Description</th>
                            <th style='width: 15%;'>Quantity</th>
                            <th style='width: 15%;'>Unit Price</th>
                            <th style='width: 15%;'>Amount</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <div class='service-description'>{EscapeHtml(description)}</div>
                            </td>
                            <td>1</td>
                            <td>{FormatCurrency(amount)}</td>
                            <td>{FormatCurrency(amount)}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Summary -->
            <div class='summary'>
                <div class='summary-row total-row'>
                    <span class='summary-label'>Total Amount</span>
                    <span class='summary-value'>{FormatCurrency(amount)}</span>
                </div>
            </div>

            <!-- Footer -->
            <div class='footer'>
                <div class='footer-content'>
                    <h3>MVM Cleaning</h3>
                    <p>
                        Thank you for your business!
                    </p>
                    <div class='thank-you'>
                        If you have any questions, please contact us.
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>");

    return html.ToString();
}


// Helper method to escape HTML (important for security)
private string EscapeHtml(string input)
{
    if (string.IsNullOrEmpty(input))
        return string.Empty;
    
    return System.Net.WebUtility.HtmlEncode(input);
}    // Helper to format currency based on your region (defaulting to generic currency here)
    private string FormatCurrency(decimal amount)
    {
        // You can change "C" to a specific culture like CultureInfo.GetCultureInfo("en-GB") for £
        return amount.ToString("C"); 
    }
}