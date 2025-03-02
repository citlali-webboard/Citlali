using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices.Marshalling;

namespace Citlali.Controllers;

[Route("event")]
public class EventController : Controller
{
    private readonly ILogger<EventController> _logger;
    private readonly EventService _eventService;
    private readonly UserService _userService;
    private readonly NotificationService _notificationService;
    private readonly Supabase.Client _supabaseClient;
    private readonly Configuration _configuration;


    public EventController(ILogger<EventController> logger, EventService eventService, UserService userService, Supabase.Client supabaseClient, NotificationService notificationService, Configuration configuration)
    {
        _logger = logger;
        _eventService = eventService;
        _userService = userService;
        _supabaseClient = supabaseClient;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return RedirectToAction("Explore");
    }

    [HttpGet("create")]
    [Authorize]
    public async Task<IActionResult> Create(CreateEventViewModel createEventViewModel)
    {
        // CreateEventViewModel createEventViewModel = new();
        createEventViewModel.Tags = await _eventService.GetTags();
        createEventViewModel.LocationTags = await _eventService.GetLocationTags();

        return View(createEventViewModel);
    }

    [HttpPost("createEvent")]
    [Authorize]
    public async Task<IActionResult> CreateEvent(CreateEventViewModel createEventViewModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("All fields are required");
            }
            if (createEventViewModel.PostExpiryDate < DateTime.UtcNow || createEventViewModel.EventDate < DateTime.UtcNow)
            {
                throw new Exception("Post expiry date and event date must be in the future");
            }
            if (createEventViewModel.EventDate <= createEventViewModel.PostExpiryDate)
            {
                throw new Exception("Event date must be after post expiry date");
            }
            if (createEventViewModel.Cost < 0)
            {
                throw new Exception("Cost must be a positive number");
            }
            if (createEventViewModel.MaxParticipant < 0)
            {
                throw new Exception("Max participants must be a positive number");
            }
            if (createEventViewModel.EventLocationTagId == Guid.Empty || createEventViewModel.EventCategoryTagId == Guid.Empty)
            {
                throw new Exception("Location and Category tags are required");
            }

            var newEvent = await _eventService.CreateEvent(createEventViewModel);
            return RedirectToAction("detail", new { id = newEvent.EventId });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            // createEventViewModel.Tags = await _eventService.GetTags();
            // createEventViewModel.LocationTags = await _eventService.GetLocationTags();
            return RedirectToAction("create", createEventViewModel);
        }
    }

    [HttpPost("delete/{eventId}")]
    [Authorize]
    public async Task<IActionResult> DeleteEvent(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.DeleteEvent(Guid.Parse(eventId));
            return RedirectToAction("explore");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to delete this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("archive/{eventId}")]
    [Authorize]
    public async Task<IActionResult> ArchiveEvent(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.ArchiveEvent(Guid.Parse(eventId));
            return RedirectToAction("explore");
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to archive this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("/status/open/{eventId}")]
    [Authorize]
    public async Task<IActionResult> OpenEvent(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.OpenEvent(Guid.Parse(eventId));
            return RedirectToAction("manage", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to manage this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("/status/close/{eventId}")]
    [Authorize]
    public async Task<IActionResult> CloseEvent(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.CloseEvent(Guid.Parse(eventId));
            return RedirectToAction("manage", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to manage this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }


    [HttpGet("detail/{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid Event id");
            }

            EventDetailViewModel eventDetailViewModel = await _eventService.GetEventDetailPage(Guid.Parse(id));

            return View(eventDetailViewModel);
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (JoinOwnerException) {
            return RedirectToAction("manage", new { eventId = id });
        }
        catch (UserAlreadyRegisteredException) {
            return RedirectToAction("status", new { eventId = id });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("join")]
    [Authorize]
    public async Task<IActionResult> JoinEvent(JoinEventModel joinEventModel)
    {
        try {
            var RequestJoinEvent = await _eventService.JoinEvent(joinEventModel);
            return RedirectToAction("status", new { eventId = joinEventModel.EventId });
        }
        catch (EventClosedException) {
            return RedirectToAction("detail", new { eventId = joinEventModel.EventId });
        }
    }


    [HttpGet("explore")]
    public async Task<IActionResult> Explore(int page = 1, int pageSize = 10, string sortBy = "newest")
    {
        try
        {
            // Store sortBy in ViewBag for active button highlighting
            ViewBag.SortBy = sortBy;

            var eventsTask = _eventService.GetPaginatedEvents((page - 1) * pageSize, (page * pageSize) - 1, sortBy);
            var eventsCountTask = _eventService.GetEventsExactCount();
            var tagsTask = _eventService.GetTags();

            await Task.WhenAll(eventsTask, eventsCountTask, tagsTask);

            var events = await eventsTask;
            var eventsCount = await eventsCountTask;
            var tags = (await tagsTask).ToArray();

            var briefCardDatas = await _eventService.EventsToBriefCardArray(events);
            var locations = (await _eventService.GetLocationTags()).ToArray();

            var model = new EventExploreViewModel
            {
                EventBriefCardDatas = briefCardDatas,
                Tags = tags,
                Locations = locations,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(eventsCount / (double)pageSize)
            };

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpGet("tag/{id}")]
    public async Task<IActionResult> Tag(string id, int page = 1, int pageSize = 10, string sortBy = "newest")
    {
        try
        {
            if (!Guid.TryParse(id, out var tagId))
            {
                throw new Exception("Invalid Tag id");
            }

            ViewBag.SortBy = sortBy;

            var eventsTask = _eventService.GetEventsByTagId(tagId);
            var tagsTask = _eventService.GetTags();
            var locationsTask = _eventService.GetLocationTags();
            var exploreTagTask = _eventService.GetTagById(tagId);
            var eventCountTask = _eventService.GetEventCountByTagId(tagId);
            var tagFollowersTask = _eventService.GetTagFollowersCountByTagId(tagId);

            await Task.WhenAll(eventsTask, tagsTask, locationsTask, exploreTagTask, eventCountTask, tagFollowersTask);

            var events = await eventsTask;
            var tags = (await tagsTask).ToList();
            var locations = await locationsTask;
            var exploreTag = await exploreTagTask ?? new EventCategoryTag();
            var eventCount = await eventCountTask;
            var tagFollowers = await tagFollowersTask;

            // Apply sorting before pagination
            switch (sortBy)
            {
                case "oldest":
                    events = events.OrderBy(e => e.CreatedAt).ToList();
                    break;
                case "date":
                    events = events.OrderBy(e => e.EventDate).ToList();
                    break;
                case "popularity":
                    var eventCountPairs = new List<(Event Event, int Count)>();
                    var registrationCountTasks = events.Select(ev => 
                        Task.Run(async () => {
                            var count = await _eventService.GetRegistrationCountByEventId(ev.EventId);
                            return (ev, count);
                        })).ToArray();
                    
                    var eventWithCounts = await Task.WhenAll(registrationCountTasks);
                    
                    events = eventWithCounts
                        .OrderByDescending(p => p.count)
                        .Select(p => p.ev)
                        .ToList();
                    break;
                case "newest":
                default:
                    events = events.OrderByDescending(e => e.CreatedAt).ToList();
                    break;
            }

            var paginatedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

            var eventDataTasks = new Task<EventBriefCardData>[paginatedEvents.Length];
            
            for (int i = 0; i < paginatedEvents.Length; i++)
            {
                var ev = paginatedEvents[i];
                eventDataTasks[i] = ProcessEventDataAsync(ev);
            }

            // Wait for all event processing to complete
            var paginatedEventsCardData = await Task.WhenAll(eventDataTasks);

            // Check if current user is following the tag
            bool isFollowing = false;
            var currentUser = _userService.CurrentSession.User;
            if (currentUser != null && currentUser.Id != null)
            {
                isFollowing = await _userService.IsFollowingTag(tagId);
            }

            // Create and return the view model
            var model = new TagEventExploreViewModel
            {
                TagId = exploreTag.EventCategoryTagId,
                TagName = exploreTag.EventCategoryTagName,
                TagEmoji = exploreTag.EventCategoryTagEmoji,
                EventCount = eventCount,
                TagFollowers = tagFollowers,
                IsFollowing = isFollowing,
                Tags = tags,
                Locations = locations,
                EventBriefCardDatas = paginatedEventsCardData,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(events.Count / (double)pageSize)
            };

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    private async Task<EventBriefCardData?> ProcessEventDataAsync(Event ev)
    {
        var creatorTask = _userService.GetUserByUserId(ev.CreatorUserId);
        var locationTagTask = _eventService.GetLocationTagById(ev.EventLocationTagId);
        var categoryTagTask = _eventService.GetTagById(ev.EventCategoryTagId);
        var registrationCountTask = _eventService.GetRegistrationCountByEventId(ev.EventId);

        await Task.WhenAll(creatorTask, locationTagTask, categoryTagTask, registrationCountTask);

        var creator = await creatorTask;
        if (creator == null)
            return null;

        var locationTag = await locationTagTask ?? new LocationTag();
        var categoryTag = await categoryTagTask ?? new EventCategoryTag();
        var registrationCount = await registrationCountTask;

        return new EventBriefCardData
        {
            EventId = ev.EventId,
            EventTitle = ev.EventTitle,
            EventDescription = ev.EventDescription,
            CreatorDisplayName = creator.DisplayName,
            CreatorProfileImageUrl = creator.ProfileImageUrl,
            LocationTag = locationTag,
            EventCategoryTag = categoryTag,
            CurrentParticipant = registrationCount,
            MaxParticipant = ev.MaxParticipant,
            Cost = ev.Cost,
            EventDate = ev.EventDate,
            PostExpiryDate = ev.PostExpiryDate,
            CreatedAt = ev.CreatedAt,
        };
    }

    [HttpGet("manage/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Manage(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            var eventManagementViewModel = await _eventService.GetEventManagement(Guid.Parse(eventId));
            return View(eventManagementViewModel);
        }
        catch (UnauthorizedAccessException) {
            TempData["Error"] = "You are not authorized to manage this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException) {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpGet("status/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Status(string eventId)
    {
        try
        {
            var eventStatusViewModel = await _eventService.GetEventStatus(Guid.Parse(eventId));
            return View(eventStatusViewModel);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to view this event status";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (UserHasNotRegisteredException)
        {
            return RedirectToAction("detail", new { id = eventId });
        }
        catch (JoinOwnerException)
        {
            return RedirectToAction("detail", new { id = eventId });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("invite/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Invite(string eventId, string userId)
    {
        try {
            await _eventService.InviteUser(Guid.Parse(eventId), Guid.Parse(userId));

            return RedirectToAction("manage", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to invite users to this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (MaximumInvitationExceedException) {
            TempData["Error"] = "Maximum invitation exceed";
            return RedirectToAction("manage", new { eventId = eventId });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpPost("reject/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Reject(string eventId, string userId)
    {
        try {
            await _eventService.RejectUser(Guid.Parse(eventId), Guid.Parse(userId));
            return RedirectToAction("manage", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to rejects users to this event";
            return RedirectToAction("explore");
        }
    }

    [HttpPost("CancelRegistration")]
    [Authorize]
    public async Task<IActionResult> CancelRegistration(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.CancelRegistration(Guid.Parse(eventId));
            return RedirectToAction("detail", new { id = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to cancel this registration";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    //RejectedInvitation
    [HttpPost("RejectedInvitation")]
    [Authorize]
    public async Task<IActionResult> RejectedInvitation(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.RejectedInvitation(Guid.Parse(eventId));
            return RedirectToAction("status", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to reject this invitation";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }


    //Confirmed
    [HttpPost("Confirmed")]
    [Authorize]
    public async Task<IActionResult> Confirmed(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.ConfirmRegistration(Guid.Parse(eventId));
            return RedirectToAction("status", new { eventId = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to confirm this invitation";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }


    [HttpPost("Broadcast")]
    [Authorize]
    public async Task<IActionResult> Broadcast(string eventId, string title, string message)
    {
        try{
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            await _eventService.Broadcast(Guid.Parse(eventId), title, message);

            return RedirectToAction("manage", new { eventId = eventId });

        }catch(Exception e){
            TempData["Error"] = e.Message;

            return RedirectToAction("manage", new { eventId = eventId });
        }
    }




    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

<<<<<<< Updated upstream

=======
    [HttpGet("followed")]
    public async Task<IActionResult> Followed(int page = 1, int pageSize = 10)
    {
        try
        {
            if (_userService.CurrentSession.User == null || string.IsNullOrEmpty(_userService.CurrentSession.User.Id))
            {
                return RedirectToAction("SignIn", "Auth");
            }
            
            var userId = Guid.Parse(_userService.CurrentSession.User.Id);
            
            // Start tasks in parallel - using GetFollowedTags instead of HasFollowedTags
            var followedTagsTask = _userService.GetFollowedTags(userId.ToString());
            var tagsTask = _eventService.GetTags();
            var eventsTask = _eventService.GetEventsFromFollowed(userId);
            
            // Wait for all initial tasks to complete
            await Task.WhenAll(followedTagsTask, tagsTask, eventsTask);
            
            var followedTags = await followedTagsTask;
            var tags = (await tagsTask).ToArray();
            var events = await eventsTask;
            
            // Check if user has followed tags/users
            bool hasFollowedTags = followedTags != null && followedTags.Count > 0;
            bool hasFollowedUsers = await _userService.GetFollowingCount(userId) > 0;
            
            // If no events found, return early with empty arrays
            if (events.Count == 0)
            {
                var emptyModel = new FollowedExploreViewModel
                {
                    EventBriefCardDatas = Array.Empty<EventBriefCardData>(),
                    Tags = tags,
                    CurrentPage = 1,
                    TotalPage = 0,
                    HasFollowedTags = hasFollowedTags,
                    HasFollowedUsers = hasFollowedUsers
                };
                return View(emptyModel);
            }
            
            var paginatedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
            
            // Create tasks for all creators, location tags, and event tags at once
            var cardTasks = new List<Task>();
            var paginatedEventsCardData = new EventBriefCardData[paginatedEvents.Length];
            
            for (int i = 0; i < paginatedEvents.Length; i++)
            {
                var ev = paginatedEvents[i];
                var index = i; // Capture the current index for the closure
                
                var creatorTask = _userService.GetUserByUserId(ev.CreatorUserId);
                var locationTagTask = _eventService.GetLocationTagById(ev.EventLocationTagId);
                var categoryTagTask = _eventService.GetTagById(ev.EventCategoryTagId);
                var participantCountTask = _eventService.GetRegistrationCountByEventId(ev.EventId);
                
                // Create a composite task that processes all needed data for this card
                var cardTask = Task.WhenAll(creatorTask, locationTagTask, categoryTagTask, participantCountTask)
                    .ContinueWith(t => {
                        var creator = creatorTask.Result ?? new User
                        {
                            DisplayName = "Unknown User",
                            ProfileImageUrl = "/images/default-profile.png",
                        };
                        
                        paginatedEventsCardData[index] = new EventBriefCardData
                        {
                            EventId = ev.EventId,
                            EventTitle = ev.EventTitle,
                            EventDescription = ev.EventDescription,
                            CreatorDisplayName = creator.DisplayName,
                            CreatorProfileImageUrl = creator.ProfileImageUrl,
                            LocationTag = locationTagTask.Result ?? new LocationTag(),
                            EventCategoryTag = categoryTagTask.Result ?? new EventCategoryTag(),
                            CurrentParticipant = participantCountTask.Result,
                            MaxParticipant = ev.MaxParticipant,
                            Cost = ev.Cost,
                            EventDate = ev.EventDate,
                            PostExpiryDate = ev.PostExpiryDate,
                            CreatedAt = ev.CreatedAt,
                        };
                    });
                    
                cardTasks.Add(cardTask);
            }
            
            await Task.WhenAll(cardTasks);

            var model = new FollowedExploreViewModel
            {
                Tags = tags,
                EventBriefCardDatas = paginatedEventsCardData,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(events.Count / (double)pageSize),
                HasFollowedTags = hasFollowedTags,
                HasFollowedUsers = hasFollowedUsers
            };

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("Explore");
        }
    }

    
>>>>>>> Stashed changes
}