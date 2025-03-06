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
    public async Task<IActionResult> History(string status = "all")
    {
        try {
            ViewData["SelectedStatus"] = status;
            
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
        var followingCount = await _userService.GetFollowingCount(user.UserId);
        var followersCount = await _userService.GetFollowersCount(user.UserId);
        var isFollowing = currentUser != null && await _userService.IsFollowing(Guid.Parse(currentUser.Id ?? string.Empty), user.UserId);
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
            FollowingCount = followingCount,
            FollowersCount = followersCount,
            IsCurrentUser = isCurrentUser,
            IsFollowing = isFollowing,
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
    
    [HttpPost("follow/{username}")]
    [Authorize]
    public async Task<IActionResult> Follow(string username)
    {
        try {

            var currentUser = _supabaseClient.Auth.CurrentUser;
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException();
            }

            var userToFollow = await _userService.GetUserByUsername(username);
            if (userToFollow == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("explore", "event");
            }

            var userId = currentUser.Id;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException();
            }

            await _userService.FollowUser(Guid.Parse(userId), userToFollow.UserId);

            var followersCount = await _userService.GetFollowersCount(userToFollow.UserId);
            return Json(new { followersCount });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You must be logged in to follow users.";
            return RedirectToAction("signin", "auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error following user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("unfollow/{username}")]
    [Authorize]
    public async Task<IActionResult> Unfollow(string username)
    {
        try {
            var currentUser = _userService.CurrentSession.User;
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException();
            }

            var userToUnfollow = await _userService.GetUserByUsername(username);
            if (userToUnfollow == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var userId = currentUser.Id;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException();
            }

            await _userService.UnfollowUser(Guid.Parse(userId), userToUnfollow.UserId);

            var followersCount = await _userService.GetFollowersCount(userToUnfollow.UserId);
            return Json(new { followersCount });
        } catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You must be logged in to unfollow users.";
            return RedirectToAction("signin", "auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfollowing user");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("followTag/{tagId}")]
    [Authorize]
    public async Task<IActionResult> FollowTag(Guid tagId)
    {
        try {
            var currentUser = _userService.CurrentSession.User;
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException();
            }

            var success = await _userService.FollowTag(tagId);
            return Json(new { success });
        } catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You must be logged in to follow tags.";
            return RedirectToAction("signin", "auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error following tag");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("unfollowTag/{tagId}")]
    [Authorize]
    public async Task<IActionResult> UnfollowTag(Guid tagId)
    {
        try {
            var currentUser = _userService.CurrentSession.User;
            if (currentUser == null)
            {
                throw new UnauthorizedAccessException();
            }

            var success = await _userService.UnfollowTag(tagId);
            return Json(new { success });
        } catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You must be logged in to follow tags.";
            return RedirectToAction("signin", "auth");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unfollowing tag");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("followedTags")]
    [Authorize]
    public async Task<IActionResult> GetFollowedTags()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var followedTags = await _userService.GetFollowedTags(currentUser.Id ?? string.Empty);
        return Json(followedTags);
    }

    [HttpGet("isFollowingTag/{tagId}")]
    [Authorize]
    public async Task<IActionResult> IsFollowingTag(Guid tagId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var isFollowing = await _userService.IsFollowingTag(tagId);
        return Json(new { isFollowing });
    }

    [HttpGet("follows")]
    [Authorize]
    public async Task<IActionResult> Follows(string ActiveTab = "followers")
    {
        if (ActiveTab != "followers" && ActiveTab != "following")
        {
            ActiveTab = "followers"; 
        }
        
        ViewData["ActiveTab"] = ActiveTab;

        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null || string.IsNullOrEmpty(currentUser.Id))
        {
            return RedirectToAction("SignIn", "Auth", new { returnUrl = Url.Action("Follows", "User") });
        }

        var user = await _userService.GetUserByUserId(Guid.Parse(currentUser.Id));
        
        if (user != null)
        {
            return RedirectToAction("Follows", new { username = user.Username, ActiveTab });
        }

        return RedirectToAction("SignIn", "Auth", new { returnUrl = Url.Action("Follows", "User") });
    }

    [HttpGet("follows/{username}")]
    public async Task<IActionResult> Follows(string username, string ActiveTab = "followers")
    {
        if (ActiveTab != "followers" && ActiveTab != "following")
        {
            ActiveTab = "followers"; 
        }
        
        ViewData["ActiveTab"] = ActiveTab;

        try {
            var user = await _userService.GetUserByUsername(username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("explore", "event");
            }

            var followers = await _userService.GetFollowers(user.UserId);
            var followingUsers = await _userService.GetFollowingUsers(user.UserId);
            var followingTags = await _userService.GetFollowingTags(user.UserId);
            var isCurrentUser = _userService.CurrentSession.User?.Id == user.UserId.ToString();
            var model = new FollowViewModel
            {
                User = user,
                IsCurrentUser = isCurrentUser,
                Followers = followers,
                Following = new FollowingModel
                {
                    FollowingUsers = followingUsers,
                    FollowedTags = followingTags
                }
            };

            return View(model);
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("explore", "event");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = "Something went wrong. Please try again.";
            return RedirectToAction("explore", "event");
        }

    }

    [HttpPost("removeFollower/{username}")]
    [Authorize]
    public async Task<IActionResult> RemoveFollower(string username)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var followerToRemove = await _userService.GetUserByUsername(username);
        if (followerToRemove == null)
        {
            return NotFound();
        }

        var userId = currentUser.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // This removes the follower (they will no longer follow the current user)
        await _userService.UnfollowUser(followerToRemove.UserId, Guid.Parse(userId));

        return Ok();
    }

}