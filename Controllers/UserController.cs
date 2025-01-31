using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;

namespace Citlali.Controllers;

[Route("user")]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly Supabase.Client _supabaseClient;
    private readonly UserService _userService;

    public UserController(ILogger<UserController> logger, Supabase.Client supabaseClient, UserService userService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _userService = userService;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return RedirectToAction("Onboarding");
    }

    [HttpGet("onboarding")]
    [Authorize]
    public IActionResult Onboarding()
    {
        if (_userService.RedirectToOnboarding()) {
            RedirectToAction("Onboarding");
        }
        return View();
    }

    [HttpPost("onboarding")]
    [Authorize]
    public async Task<IActionResult> Create(UserOnboardingDto user)
    {
        if (!_userService.RedirectToOnboarding()) {
            RedirectToAction("Profile");
        }
        var userCreated = await _userService.CreateUser(user);
        return RedirectToAction("Profile", new { userId = userCreated.UserId });
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> Profile(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Onboarding");
        }

        var user = await _userService.GetUserByUserId(Guid.Parse(userId));
        return View(user);
    }
}
