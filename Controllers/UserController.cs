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

    public IActionResult Index()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }
        return RedirectToAction("Onboarding");
    }

    [HttpGet("onboarding")]
    [Authorize]
    public async Task<IActionResult> Onboarding()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        if (!await _userService.RedirectToOnboarding()) {
            return RedirectToAction("Profile");
        }

        return View();
    }

    [HttpPost("onboarding")]
    [Authorize]
    public async Task<IActionResult> Create(UserOnboardingDto user)
    {
        if (!await _userService.RedirectToOnboarding()) {
            RedirectToAction("Profile");
        }
        var userCreated = await _userService.CreateUser(user);
        if (userCreated == null) {
            return RedirectToAction("Onboarding");
        }
        return RedirectToAction("Profile");
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var currentUser = _supabaseClient.Auth.CurrentUser;

            if (currentUser == null)
            {
                Console.WriteLine("User is not authenticated.");
                return RedirectToAction("SignIn", "Auth");
            }

            var userId = currentUser.Id;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("User ID is null");
                return RedirectToAction("Onboarding");
            }

            var user = await _userService.GetUserByUserId(Guid.Parse(userId));

            if (user == null)
            {
                Console.WriteLine("User not found in DB. Redirecting to SignIn.");
                return RedirectToAction("SignIn", "Auth");
            }

            return View(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return RedirectToAction("SignIn", "Auth");
        }
    }

    [HttpGet("profile/edit")]
    [Authorize]
    public async Task<IActionResult> ProfileEdit()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var user = await _userService.GetUserByUserId(Guid.Parse(currentUser.Id));
        if (user == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        var dto = new UserOnboardingDto
        {
            UserId = user.UserId,
            Email = user.Email,
            ProfileImageUrl = user.ProfileImageUrl,
            DisplayName = user.DisplayName,
            UserBio = user.UserBio,
            CreatedAt = user.CreatedAt,
            Deleted = user.Deleted
        };
        return View(dto);
    }

    [HttpPost("edit")]
    [Authorize]
    public async Task<IActionResult> ProfileEdit(UserOnboardingDto userOnboardingDto)
    {
        await _userService.EditUser(userOnboardingDto);
        return RedirectToAction("Profile");
    }
}
