using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace Booking.Infrastructure.Email;

public class EmailService
{
	private readonly string _apiKey;

	public EmailService(IConfiguration configuration)
	{
		_apiKey = configuration["SendGrid:ApiKey"];
	}

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            Console.WriteLine("SendGrid API key is not configured. Set SendGrid:ApiKey in appsettings.json or environment variables.");
            return;
        }

        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress("lorenamalaj7@gmail.com", "Lorena");
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);

            var response = await client.SendEmailAsync(msg);

            // Log status and response body for diagnostics
            var respBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"SendGrid response: {(int)response.StatusCode} {response.StatusCode}");
            if (!string.IsNullOrWhiteSpace(respBody))
                Console.WriteLine($"SendGrid response body: {respBody}");

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Email sent successfully via SendGrid.");
            }
            else
            {
                Console.WriteLine("SendGrid failed to send email. Check SendGrid dashboard, verified senders, and API key permissions.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while sending email via SendGrid: {ex.Message}");
        }
    }
}