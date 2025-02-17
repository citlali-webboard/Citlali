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

    public EventController(ILogger<EventController> logger, EventService eventService, UserService userService)
    {
        _logger = logger;
        _eventService = eventService;
        _userService = userService;
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


    [HttpGet("detail/{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid Event id");
            }

            EventDetailViewModel eventDetailViewModel = await _eventService.GetEventDetail(Guid.Parse(id));
            return View(eventDetailViewModel);

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
            return RedirectToAction("explore");
        }
        catch (UserAlreadyRegisteredException) {
            TempData["Error"] = "You have already registered for this event";
            return RedirectToAction("detail", new { id = joinEventModel.EventId });
        }
        catch (JoinOwnerException) {
            TempData["Error"] = "You cannot join your own event";
            return RedirectToAction("detail", new { id = joinEventModel.EventId });
        }
        catch (Exception e) {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }


    [HttpGet("explore")]
    public async Task<IActionResult> Explore(int page = 1, int pageSize = 10)
    {
        var events = await _eventService.GetAllEvents();
        var paginatedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        var paginatedEventsCardData = new EventBriefCardData[paginatedEvents.Length];

        for (int i = 0; i < paginatedEvents.Length; i++)
        {
            var ev = paginatedEvents[i];
            var creator = await _userService.GetUserByUserId(ev.CreatorUserId);
            if (creator == null)
            {
                continue;
            }

            paginatedEventsCardData[i] = new EventBriefCardData
            {
                EventId = ev.EventId,
                EventTitle = ev.EventTitle,
                EventDescription = ev.EventDescription,
                CreatorDisplayName = creator.DisplayName,
                CreatorProfileImageUrl = creator.ProfileImageUrl,
                LocationTag = await _eventService.GetLocationTagById(ev.EventLocationTagId) ?? new LocationTag(),
                EventCategoryTag = await _eventService.GetTagById(ev.EventCategoryTagId) ?? new EventCategoryTag(),
                CurrentParticipant = await _eventService.GetRegistrationCountByEventId(ev.EventId),
                MaxParticipant = ev.MaxParticipant,
                Cost = ev.Cost,
                EventDate = ev.EventDate,
                PostExpiryDate = ev.PostExpiryDate,
                CreatedAt = ev.CreatedAt,
            };
        }

        var tags = (await _eventService.GetTags()).ToArray();

        var model = new EventExploreViewModel
        {
            EventBriefCardDatas = paginatedEventsCardData,
            Tags = tags,
            CurrentPage = page,
            TotalPage = (int)Math.Ceiling(events.Count() / (double)pageSize)
        };

        return View(model);
    }

    [HttpGet("tag/{id}")]
    public async Task<IActionResult> Tag(string id, int page = 1, int pageSize = 10)
    {
        try
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid Tag id");
            }

            var events = await _eventService.GetEventsByTagId(Guid.Parse(id));
            var paginatedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

            var paginatedEventsCardData = new EventBriefCardData[paginatedEvents.Length];

            for (int i = 0; i < paginatedEvents.Length; i++)
            {
                var ev = paginatedEvents[i];
                var creator = await _userService.GetUserByUserId(ev.CreatorUserId);
                if (creator == null)
                {
                    continue;
                }

                paginatedEventsCardData[i] = new EventBriefCardData
                {
                    EventId = ev.EventId,
                    EventTitle = ev.EventTitle,
                    EventDescription = ev.EventDescription,
                    CreatorDisplayName = creator.DisplayName,
                    CreatorProfileImageUrl = creator.ProfileImageUrl,
                    LocationTag = await _eventService.GetLocationTagById(ev.EventLocationTagId) ?? new LocationTag(),
                    EventCategoryTag = await _eventService.GetTagById(ev.EventCategoryTagId) ?? new EventCategoryTag(),
                    CurrentParticipant = await _eventService.GetRegistrationCountByEventId(ev.EventId),
                    MaxParticipant = ev.MaxParticipant,
                    Cost = ev.Cost,
                    EventDate = ev.EventDate,
                    PostExpiryDate = ev.PostExpiryDate,
                    CreatedAt = ev.CreatedAt,
                };
            }

            var tags = (await _eventService.GetTags()).ToArray();

            var model = new TagEventExploreViewModel
            {
                EventBriefCardDatas = paginatedEventsCardData,
                Tags = tags,
                CurrentPage = page,
                TotalPage = (int)Math.Ceiling(events.Count() / (double)pageSize)
            };
            
            var exploreTag = await _eventService.GetTagById(Guid.Parse(id))?? new EventCategoryTag();

            model.TagId = exploreTag.EventCategoryTagId;
            model.TagName = exploreTag.EventCategoryTagName;
            model.TagEmoji = exploreTag.EventCategoryTagEmoji;

            return View(model);
        }
        catch (Exception e)
        {
            TempData["Error"] = e.Message;
            return RedirectToAction("explore");
        }
    }

    [HttpGet("manage/{eventId}")]
    [Authorize]
    public async Task<IActionResult> Manage(string eventId)
    {
        try
        {
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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    
}