using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Citlali.Services
{
    public class RazorViewToStringRenderer
    {
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedServiceProvider = scope.ServiceProvider;
                var viewEngine = scopedServiceProvider.GetRequiredService<IRazorViewEngine>();
                var tempDataProvider = scopedServiceProvider.GetRequiredService<ITempDataProvider>();

                var actionContext = GetActionContext(scopedServiceProvider);
                var view = FindView(viewEngine, actionContext, viewName);

                using (var output = new StringWriter())
                {
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
                            tempDataProvider),
                        output,
                        new HtmlHelperOptions());

                    await view.RenderAsync(viewContext);

                    return output.ToString();
                }
            }
        }

        private IView FindView(IRazorViewEngine viewEngine, ActionContext actionContext, string viewName)
        {
            var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage: true);
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

        private ActionContext GetActionContext(IServiceProvider serviceProvider)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}