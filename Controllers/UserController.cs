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
    private readonly Configuration _configuration;

    public UserController(ILogger<UserController> logger, Supabase.Client supabaseClient, UserService userService, Configuration configuration)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _userService = userService;
        _configuration = configuration;
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

        ViewBag.Email = currentUser.Email;
        ViewBag.ProfileImageUrl = _configuration.User.DefaultProfileImage;

        return View();
    }

    [HttpPost("onboarding")]
    [Authorize]
    public async Task<IActionResult> Create(UserOnboardingDto user)
    {
        try {
            if (!await _userService.RedirectToOnboarding()) {
                return RedirectToAction("Profile");
            }

            if (string.IsNullOrEmpty(user.DisplayName)) {
                TempData["Error"] = "Display name is required.";
                return RedirectToAction("Onboarding");
            }

            if (await _userService.GetUserByUsername(user.Username) != null) {
                TempData["Error"] = "Username is already taken.";
                return RedirectToAction("Onboarding");
            }

            var userCreated = await _userService.CreateUser(user);
            if (userCreated == null) {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Onboarding");
            }
            return RedirectToAction("Profile");
        } 
        catch (InvalidUsernameException ex) {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Onboarding");
        }
        catch (Exception ex) {
            Console.WriteLine(ex.Message);
            TempData["Error"] = "Something went wrong. Please try again.";
            return RedirectToAction("Onboarding");
        }

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
                Console.WriteLine("User not found in DB");
                return RedirectToAction("Onboarding");
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
