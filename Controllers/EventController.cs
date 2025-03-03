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


    public EventController(ILogger<EventController> logger, EventService eventService, UserService userService, Supabase.Client supabaseClient, NotificationService notificationService)
    {
        _logger = logger;
        _eventService = eventService;
        _userService = userService;
        _supabaseClient = supabaseClient;
        _notificationService = notificationService;
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

    [HttpPost("editEvent/{eventId}")]
    [Authorize]
    public async Task<IActionResult> EditEvent(string eventId, CreateEventViewModel createEventViewModel)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

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

            await _eventService.EditEvent(Guid.Parse(eventId), createEventViewModel);
            return RedirectToAction("detail", new { id = eventId });
        }
        catch (UnauthorizedAccessException) 
        {
            TempData["Error"] = "You are not authorized to edit this event";
            return RedirectToAction("explore");
        }
        catch (KeyNotFoundException)
        {
            TempData["Error"] = "Event not found";
            return RedirectToAction("explore");
        }
        catch (MaximumParticipantExceedException)
        {
            TempData["Error"] = "Maximum participants exceed";
            return RedirectToAction("edit", new { eventId = eventId });
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("edit", new { eventId = eventId });
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
            var eventsTrendingTask = _eventService.GetTrendingEvents();
            var popularTagsTask = _eventService.GetPopularTags();
            var superstarsTask = _userService.GetSuperstars();

            await Task.WhenAll(eventsTask, eventsCountTask, tagsTask, eventsTrendingTask,  popularTagsTask, superstarsTask);

            var events = await eventsTask;
            var eventsCount = await eventsCountTask;
            var tags = (await tagsTask).ToArray();
            var eventsTrending = (await eventsTrendingTask).ToArray();
            var popularTags = (await popularTagsTask).ToArray();
            var superstars = (await superstarsTask).ToArray();

            var briefCardDatas = await _eventService.EventsToBriefCardArray(events);
            var locations = (await _eventService.GetLocationTags()).ToArray();

            var model = new EventExploreViewModel
            {
                EventBriefCardDatas = briefCardDatas,
                Tags = tags,
                Locations = locations,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(eventsCount / (double)pageSize),
                TrendingEvents = eventsTrending,
                PopularTags = popularTags,
                Superstars = superstars
            };

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            Console.WriteLine(e);
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
                TotalPage = (int)Math.Ceiling(events.Count() / (double)pageSize)
            };

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpGet("location/{id}")]
    public async Task<IActionResult> Location(string id, int page = 1, int pageSize = 10, string sortBy = "newest")
    {
        try
        {
            if (!Guid.TryParse(id, out var locationId))
            {
                throw new Exception("Invalid Location id");
            }

            ViewBag.SortBy = sortBy;

            // Create tasks for parallel execution of independent queries
            var eventsTask = _eventService.GetEventsByLocationId(locationId);
            var tagsTask = _eventService.GetTags();
            var locationsTask = _eventService.GetLocationTags();
            var exploreLocationTask = _eventService.GetLocationTagById(locationId);
            var eventCountTask = _eventService.GetEventCountByLocationId(locationId);
            await Task.WhenAll(eventsTask, tagsTask, locationsTask, exploreLocationTask, eventCountTask);

            var events = await eventsTask;
            var tags = (await tagsTask).ToList();
            var locations = await locationsTask;
            var exploreLocation = await exploreLocationTask ?? new LocationTag(); // Changed to LocationTag
            var eventCount = await eventCountTask;

            switch (sortBy)
            {
                case "oldest":
                    events = events.OrderBy(e => e.CreatedAt).ToList();
                    break;
                case "date":
                    events = events.OrderBy(e => e.EventDate).ToList();
                    break;
                case "popularity":
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

            // Create and return the view model using LocationEventExploreViewModel
            var model = new LocationEventExploreViewModel
            {
                LocationId = exploreLocation.LocationTagId, // Updated property names
                LocationName = exploreLocation.LocationTagName,
                EventCount = eventCount,
                Tags = tags,
                Locations = locations.ToList(),
                EventBriefCardDatas = paginatedEventsCardData,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(events.Count() / (double)pageSize)
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

    [HttpGet("edit/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Edit(string eventId)
    {
        try
        {
            if (!Guid.TryParse(eventId, out _))
            {
                throw new Exception("Invalid Event id");
            }

            var eventEditViewModel = await _eventService.GetEventEditPage(Guid.Parse(eventId));
            return View(eventEditViewModel);
        }
        catch (UnauthorizedAccessException)
        {
            TempData["Error"] = "You are not authorized to edit this event";
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


}