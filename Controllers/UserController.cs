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
    private readonly EventService _eventService;

    public UserController(ILogger<UserController> logger, Supabase.Client supabaseClient, UserService userService, Configuration configuration, EventService eventService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _userService = userService;
        _configuration = configuration;
        _eventService = eventService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            return RedirectToAction("explore", "event");
        }

        var dbUser = await _userService.GetUserByUserId(Guid.Parse(currentUser.Id));
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
        var currentUser = _supabaseClient.Auth.CurrentUser;
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

    [HttpGet("{username}")]
    public async Task<IActionResult> Profile(string username)
    {
        var user = await _userService.GetUserByUsername(username);
        if (user == null)
        {
            return Content("User not found.");
        }

        var currentUser = _supabaseClient.Auth.CurrentUser;
        var isCurrentUser = currentUser != null && currentUser.Id == user.UserId.ToString();

        var userEvents = await _eventService.GetEventsByUserId(user.UserId);
        var userEventBriefCards = new List<EventBriefCardData>();
        foreach (var userEvent in userEvents)
        {
            var creator = await _userService.GetUserByUserId(userEvent.CreatorUserId);
            if (creator == null)
            {
                continue;
            }

            var eventTag = await _eventService.GetTagById(userEvent.EventCategoryTagId);
            var locationTag = await _eventService.GetLocationTagById(userEvent.EventLocationTagId);


            userEventBriefCards.Add(new EventBriefCardData
            {
                EventId = userEvent.EventId,
                EventTitle = userEvent.EventTitle,
                EventDescription = userEvent.EventDescription,
                CreatorDisplayName = creator.DisplayName,
                CreatorProfileImageUrl = creator.ProfileImageUrl,
                EventCategoryTag = eventTag ?? new EventCategoryTag(),
                LocationTag = locationTag ?? new LocationTag()
            });
        }

        var userViewModel = new UserViewModel
        {
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            ProfileImageUrl = user.ProfileImageUrl,
            DisplayName = user.DisplayName,
            UserBio = user.UserBio,
            IsCurrentUser = isCurrentUser,
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
            
        await _userService.EditUser(userOnboardingDto);

        return RedirectToAction("Index");
    }
}
