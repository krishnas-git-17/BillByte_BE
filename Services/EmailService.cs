using BillByte.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BillByte.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var smtp = new SmtpClient(_settings.SmtpServer)
            {
                Port = _settings.Port,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _settings.SenderEmail,
                    _settings.AppPassword
                )
            };

            var mail = new MailMessage
            {
                From = new MailAddress(
                    _settings.SenderEmail,
                    _settings.SenderName
                ),
                Subject = "BillByte Email Verification",
                Body = $"Your OTP is {otp}. Valid for 5 minutes."
            };

            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
        }
    }
}
