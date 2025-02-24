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
    private readonly EventService _eventService;
    private readonly Configuration _configuration;

    public UserController(ILogger<UserController> logger, Supabase.Client supabaseClient, UserService userService, EventService eventService, Configuration configuration)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _userService = userService;
        _eventService = eventService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null || currentUser.Id == null)
        {
            return RedirectToAction("explore", "event");
        }

        var userId = currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("explore", "event");
        }

        var dbUser = await _userService.GetUserByUserId(Guid.Parse(userId));
        if (dbUser == null)
        {
            return RedirectToAction("Onboarding");
        }

        var username = dbUser.Username;

        return RedirectToAction("Profile", new { username });
    }

    [HttpGet("onboarding")]
    [Authorize]
    public async Task<IActionResult> Onboarding()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            return RedirectToAction("SignIn", "Auth");
        }

        if (!await _userService.RedirectToOnboarding())
        {
            return RedirectToAction("Index");
        }

        ViewBag.Email = currentUser.Email;
        ViewBag.ProfileImageUrl = _configuration.User.DefaultProfileImage;
        ViewBag.Tags = await _eventService.GetTags();

        return View();
    }

    [HttpPost("onboarding")]
    [Authorize]
    public async Task<IActionResult> Create(UserOnboardingDto user)
    {
        try
        {
            if (!await _userService.RedirectToOnboarding())
            {
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(user.DisplayName))
            {
                TempData["Error"] = "Display name is required.";
                return RedirectToAction("Onboarding");
            }

            if (string.IsNullOrEmpty(user.Username))
            {
                TempData["Error"] = "Username is required.";
                return RedirectToAction("Onboarding");
            }

            if (await _userService.GetUserByUsername(user.Username) != null)
            {
                TempData["Error"] = "Username is already taken.";
                return RedirectToAction("Onboarding");
            }

            var userCreated = await _userService.CreateUser(user);
            if (userCreated == null)
            {
                TempData["Error"] = "Something went wrong. Please try again.";
                return RedirectToAction("Onboarding");
            }
            
            HttpContext.Response.Cookies.Append("ProfileImageURL", userCreated.ProfileImageUrl, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            return RedirectToAction("Index");
        }
        catch (InvalidUsernameException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Onboarding");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = "Something went wrong. Please try again.";
            return RedirectToAction("Onboarding");
        }

    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> History()
    {
        try {
            var historyList = await _eventService.GetHistory();
            return View(historyList);
        }
        catch (UnauthorizedAccessException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Event");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = "Something went wrong. Please try again.";
            return RedirectToAction("Index", "Event");
        }
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> Profile(string username)
    {
        var user = await _userService.GetUserByUsername(username);
        if (user == null)
        {
            return Content("User not found.");
        }

        var currentUser = _userService.CurrentSession.User;
        var isCurrentUser = currentUser != null && currentUser.Id == user.UserId.ToString();
        var followingCount = await _userService.GetFollowingCount(user.UserId);
        var followersCount = await _userService.GetFollowersCount(user.UserId);
        var isFollowing = currentUser != null && await _userService.IsFollowing(Guid.Parse(currentUser.Id ?? string.Empty), user.UserId);

        var events = await _eventService.GetEventsByUserId(user.UserId);
        var userEventBriefCards = (await _eventService.EventsToBriefCardArray(events)).ToList();

        var userViewModel = new UserViewModel
        {
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            ProfileImageUrl = user.ProfileImageUrl,
            DisplayName = user.DisplayName,
            UserBio = user.UserBio,
            FollowingCount = followingCount,
            FollowersCount = followersCount,
            IsCurrentUser = isCurrentUser,
            IsFollowing = isFollowing,
            UserEvents = userEventBriefCards
        };

        return View(userViewModel);
    }

    [HttpPost("edit")]
    [Authorize]
    public async Task<IActionResult> ProfileEdit(UserOnboardingDto userOnboardingDto)
    {
        if (string.IsNullOrEmpty(userOnboardingDto.DisplayName))
        {
            TempData["Error"] = "Display name is required.";
            return RedirectToAction("Index");
        }
            
        var updatedUser = await _userService.EditUser(userOnboardingDto);

        Response.Cookies.Append("ProfileImageUrl", updatedUser.ProfileImageUrl, new CookieOptions
        {
            HttpOnly = false,  
            Secure = true, 
            SameSite = SameSiteMode.Strict,  
            Expires = DateTime.UtcNow.AddDays(30)
        });

        return RedirectToAction("Index");
    }
}