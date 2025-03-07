using Citlali.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Citlali.Filters;

class OnboardingFilter(UserService userService) : IAsyncActionFilter
{
    private readonly UserService _userService = userService;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (await _userService.RedirectToOnboarding())
        {
            context.Result = new Microsoft.AspNetCore.Mvc.RedirectToActionResult("Onboarding", "User", null);
            return;
        }
        await next();
    }
}