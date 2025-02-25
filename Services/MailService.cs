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
namespace Citlali.Services;

public class MailService(
    Configuration configuration,
    SmtpClient smtpClient,
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider)
{
    private readonly Configuration _configuration = configuration;
    private readonly SmtpClient _smtpClient = smtpClient;
    private readonly MailAddress _sendFrom = new(configuration.Mail.SendAddress, configuration.Mail.SendDisplayName);
    private readonly IRazorViewEngine _viewEngine = viewEngine;
    private readonly ITempDataProvider _tempDataProvider = tempDataProvider;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    // Code from: https://github.com/aspnet/Entropy/blob/master/samples/Mvc.RenderViewToString/RazorViewToStringRenderer.cs
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName)
    {
        var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));

        throw new InvalidOperationException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider
        };
        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

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

    public async void SendNotificationEmail(MailNotificationViewModel model, string recievingAddress)
    {
        var body = await RenderViewToStringAsync("~/Views/Mail/Notification.cshtml", model);
        await Send(model, body, recievingAddress);
    }
}
