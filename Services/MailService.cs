using System.Net.Mail;
using Citlali.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Citlali.Services
{
    public class MailService(
        Configuration configuration,
        SmtpClient smtpClient,
        RazorViewToStringRenderer razorViewToStringRenderer)
    {
        private readonly Configuration _configuration = configuration;
        private readonly SmtpClient _smtpClient = smtpClient;
        private readonly MailAddress _sendFrom = new MailAddress(configuration.Mail.SendAddress, configuration.Mail.SendDisplayName);
        private readonly RazorViewToStringRenderer _razorViewToStringRenderer = razorViewToStringRenderer;

        public async Task Send(MailBaseViewModel model, string body, string recievingAddress)
        {
            MailAddress sendTo = new(recievingAddress);
            MailMessage mailMessage = new(_sendFrom, sendTo)
            {
                Subject = $"{model.Title} • Citlali",
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Body = body
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendNotificationEmail(MailNotificationViewModel model, string recievingAddress)
        {
            var body = await _razorViewToStringRenderer.RenderViewToStringAsync("~/Views/Mail/Notification.cshtml", model);
            await Send(model, body, recievingAddress);
        }
    }
}