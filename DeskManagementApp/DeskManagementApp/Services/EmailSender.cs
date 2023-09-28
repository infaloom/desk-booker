using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DeskManagementApp.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _apiKey;

        public EmailSender()
        {
            _apiKey = "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";
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
                From = new EmailAddress("deskmanagementapp@infaloom.com", "DeskManagementApp"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            return client.SendEmailAsync(msg);
        }
    }
}
