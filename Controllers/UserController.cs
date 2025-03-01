using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;

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
        var currentUser = _userService.CurrentSession.User;
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
    public async Task<IActionResult> Profile(string username, int page = 1, string filter = null, string sort = "newest")
    {
        const int pageSize = 10;
        
        var user = await _userService.GetUserByUsername(username);
        if (user == null)
        {
            return Content("User not found.");
        }

        var currentUser = _userService.CurrentSession.User;
        var isCurrentUser = currentUser != null && currentUser.Id == user.UserId.ToString();
        if (isCurrentUser) {
            HttpContext.Response.Cookies.Append("ProfileImageUrl", user.ProfileImageUrl, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(30)
            });
        }

        var events = await _eventService.GetEventsByUserId(user.UserId);
        if (!string.IsNullOrEmpty(filter))
        {
            if (filter == "active")
            {
                events = events.Where(e => e.Status == "active" && e.PostExpiryDate > DateTime.Now).ToList();
            }
            else if (filter == "closed")
            {
                events = events.Where(e => e.Status == "closed" || e.PostExpiryDate <= DateTime.Now).ToList();
            }
        }
        
        switch (sort)
        {
            case "oldest":
                events = events.OrderBy(e => e.CreatedAt).ToList();
                break;
            case "popular":
                var eventsWithCounts = new List<(Event Event, int Count)>();
                foreach (var e in events)
                {
                    var count = await _eventService.GetRegistrationCountByEventId(e.EventId);
                    eventsWithCounts.Add((e, count));
                }
                events = eventsWithCounts.OrderByDescending(x => x.Count).Select(x => x.Event).ToList();
                break;
            case "newest":
            default:
                events = events.OrderByDescending(e => e.CreatedAt).ToList();
                break;
        }
        
        // Calculate total pages
        int totalEvents = events.Count;
        int totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);
        
        // Ensure page is within valid range
        page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));
        
        // Apply pagination
        events = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var userEventBriefCards = (await _eventService.EventsToBriefCardArray(events)).ToList();

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

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Filter = filter;
        ViewBag.Sort = sort;

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
