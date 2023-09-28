using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Humanizer.Configuration;

namespace DeskManagementApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _apiKey;
        private readonly IConfiguration _configuration;
       
    public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["SendGridApiKey"];
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_apiKey, subject, htmlMessage, email);
        }

        private static Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("dimitrije@infaloom.com", "DeskManagementApp"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }
    }
}
