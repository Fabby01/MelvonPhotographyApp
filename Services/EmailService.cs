using System.Net;
using System.Net.Mail;

namespace MRMstudios.Services
{
    public interface IEmailService
    {
        Task<bool> SendBookingConfirmationAsync(string clientEmail, string clientName, int bookingId, string serviceType, DateTime bookingDate, decimal price);
        Task<bool> SendBookingNotificationToOwnerAsync(string clientName, string clientEmail, string phoneNumber, string serviceType, DateTime bookingDate, string specialNotes);
    }

    public class EmailService : IEmailService
    {
        private readonly string _ownerEmail = "mel.dimplz@gmail.com";
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendBookingConfirmationAsync(string clientEmail, string clientName, int bookingId, 
            string serviceType, DateTime bookingDate, decimal price)
        {
            try
            {
                var subject = $"Photography Booking Confirmation - ID: {bookingId}";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #f5f7fa 0%, #ffffff 100%); padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px; }}
                            .header h1 {{ color: #0d6efd; margin: 0; }}
                            .details {{ background: #f9f9f9; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #0d6efd; }}
                            .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; }}
                            .label {{ font-weight: 600; color: #666; }}
                            .value {{ color: #333; }}
                            .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0; font-size: 0.9rem; color: #666; }}
                            .btn {{ display: inline-block; background: #0d6efd; color: white; padding: 10px 20px; border-radius: 6px; text-decoration: none; margin-top: 15px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎨 MRMstudios</h1>
                                <p>Your Photography Booking is Confirmed!</p>
                            </div>

                            <p>Hi {clientName},</p>
                            <p>Thank you for booking with MRMstudios! We're excited to capture your special moments.</p>

                            <div class='details'>
                                <div class='detail-row'>
                                    <span class='label'>Confirmation ID:</span>
                                    <span class='value'>{bookingId}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Service:</span>
                                    <span class='value'>{serviceType}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Session Date:</span>
                                    <span class='value'>{bookingDate:dddd, MMMM d, yyyy}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Investment:</span>
                                    <span class='value'>${price}</span>
                                </div>
                            </div>

                            <p><strong>What's Next?</strong></p>
                            <p>Our team will contact you within 24 hours to confirm the session time and discuss any special requirements or preparation.</p>

                            <p><strong>Contact Information:</strong></p>
                            <p>
                                📧 <a href='mailto:mel.dimplz@gmail.com'>mel.dimplz@gmail.com</a><br>
                                📱 <a href='tel:+46708417437'>+46 70 841 7437</a>
                            </p>

                            <p>We look forward to working with you!</p>
                            <p>Best regards,<br><strong>MRMstudios Team</strong></p>

                            <div class='footer'>
                                <p>&copy; 2026 MRMstudios. All rights reserved.</p>
                                <p>Professional Photography Services | Sweden 🇸🇪</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(clientEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending confirmation email: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendBookingNotificationToOwnerAsync(string clientName, string clientEmail, 
            string phoneNumber, string serviceType, DateTime bookingDate, string specialNotes)
        {
            try
            {
                var subject = $"New Booking Request - {serviceType} on {bookingDate:MMM d, yyyy}";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: linear-gradient(135deg, #0d6efd 0%, #0b5ed7 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px; }}
                            .header h2 {{ margin: 0; }}
                            .details {{ background: #f9f9f9; padding: 15px; border-radius: 8px; margin: 15px 0; border-left: 4px solid #0d6efd; }}
                            .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #e0e0e0; }}
                            .detail-row:last-child {{ border-bottom: none; }}
                            .label {{ font-weight: 600; color: #666; }}
                            .value {{ color: #333; }}
                            .notes {{ background: #fff9e6; padding: 10px; border-radius: 6px; margin-top: 10px; border-left: 4px solid #ffc107; }}
                            .action {{ background: #e7f3ff; padding: 15px; border-radius: 6px; margin-top: 15px; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>📸 New Booking Request</h2>
                            </div>

                            <p>You have a new booking request for {serviceType}!</p>

                            <div class='details'>
                                <div class='detail-row'>
                                    <span class='label'>Client Name:</span>
                                    <span class='value'>{clientName}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Email:</span>
                                    <span class='value'><a href='mailto:{clientEmail}'>{clientEmail}</a></span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Phone:</span>
                                    <span class='value'><a href='tel:{phoneNumber}'>{phoneNumber}</a></span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Service:</span>
                                    <span class='value'>{serviceType}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='label'>Requested Date:</span>
                                    <span class='value'>{bookingDate:dddd, MMMM d, yyyy}</span>
                                </div>
                            </div>

                            {(!string.IsNullOrWhiteSpace(specialNotes) ? $@"
                            <div class='notes'>
                                <strong>Special Requests/Notes:</strong>
                                <p>{specialNotes}</p>
                            </div>
                            " : "")}

                            <div class='action'>
                                <p><strong>Action Required:</strong></p>
                                <p>Please contact the client within 24 hours to confirm the session time and finalize details.</p>
                            </div>

                            <p style='color: #666; font-size: 0.9rem;'>This booking has been automatically saved to your system.</p>
                        </div>
                    </body>
                    </html>
                ";

                return await SendEmailAsync(_ownerEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending owner notification email: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                // For development/demo purposes, we'll log the email instead of actually sending
                // In production, configure real SMTP settings in appsettings.json
                _logger.LogInformation($"Email to {toEmail}: {subject}");
                await Task.CompletedTask;
                
                // Uncomment below and configure for production SMTP sending:
                /*
                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password");
                    
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("your-email@gmail.com", "MRMstudios"),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                }
                */

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
