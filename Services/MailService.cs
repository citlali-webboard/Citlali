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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace Citlali.Services
{
    public class MailService : IDisposable
    {
        private readonly Configuration _configuration;
        private readonly SmtpClient _smtpClient;
        private readonly MailAddress _sendFrom;
        private readonly RazorViewToStringRenderer _razorViewToStringRenderer;
        private readonly ActionBlock<MailRequest> _mailQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public MailService(
            Configuration configuration,
            SmtpClient smtpClient,
            RazorViewToStringRenderer razorViewToStringRenderer)
        {
            _configuration = configuration;
            _smtpClient = smtpClient;
            _sendFrom = new MailAddress(configuration.Mail.SendAddress, configuration.Mail.SendDisplayName);
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _cancellationTokenSource = new CancellationTokenSource();

            // Create mail processing queue with one mail at a time
            _mailQueue = new ActionBlock<MailRequest>(
                async request => await ProcessMailRequest(request),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1,
                    CancellationToken = _cancellationTokenSource.Token
                }
            );
        }

        private class MailRequest
        {
            public MailBaseViewModel Model { get; set; }
            public string Body { get; set; }
            public string ReceivingAddress { get; set; }
            public TaskCompletionSource<bool> CompletionSource { get; set; }
        }

        private async Task ProcessMailRequest(MailRequest request)
        {
            try
            {
                MailAddress sendTo = new(request.ReceivingAddress);
                MailMessage mailMessage = new(_sendFrom, sendTo)
                {
                    Subject = $"{request.Model.Title} • Citlali",
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = true,
                    Body = request.Body
                };

                _smtpClient.Send(mailMessage);
                request.CompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                request.CompletionSource.SetException(ex);
            }
        }

        public async Task<bool> SendAsync(MailBaseViewModel model, string body, string receivingAddress)
        {
            var completionSource = new TaskCompletionSource<bool>();
            var request = new MailRequest
            {
                Model = model,
                Body = body,
                ReceivingAddress = receivingAddress,
                CompletionSource = completionSource
            };

            await _mailQueue.SendAsync(request);
            return await completionSource.Task;
        }

        // This remains synchronous for backward compatibility
        public void Send(MailBaseViewModel model, string body, string receivingAddress)
        {
            SendAsync(model, body, receivingAddress).GetAwaiter().GetResult();
        }

        public async Task<bool> SendNotificationEmail(MailNotificationViewModel model, string receivingAddress)
        {
            var body = await _razorViewToStringRenderer.RenderViewToStringAsync("~/Views/Mail/Notification.cshtml", model);
            return await SendAsync(model, body, receivingAddress);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}