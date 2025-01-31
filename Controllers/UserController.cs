using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

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
        return RedirectToAction("Register");
    }

    [HttpGet("onboarding")]
    public IActionResult Onboarding()
    {
        return View();
    }

    [HttpGet("profile/{userId}")]
    public async Task<IActionResult> Profile(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Register");
        }

        var user = await _userService.GetUserByUserId(Guid.Parse(userId));
        return View(user);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(UserRegisterDTO user)
    {
        var userCreated = await _userService.CreateUser(user, user.Password);
        return RedirectToAction("Profile", new { userId = userCreated.UserId });
    }
}
