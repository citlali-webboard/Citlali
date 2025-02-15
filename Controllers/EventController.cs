using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Citlali.Controllers;

[Route("event")]

public class EventController : Controller
{
    private readonly ILogger<EventController> _logger;
    private readonly EventService _eventService;

    public EventController(ILogger<EventController> logger, EventService eventService)
    {
        _logger = logger;
        _eventService = eventService;
    }

    public IActionResult Index()
    {
        return View();
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
        try{
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
        }catch (Exception e){
            TempData["Error"] = e.Message;
            // createEventViewModel.Tags = await _eventService.GetTags();
            // createEventViewModel.LocationTags = await _eventService.GetLocationTags();
            return RedirectToAction("create", createEventViewModel);
        }
    }


    [HttpGet("detail/{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        try{
            if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid Event id");
            }

            EventDetailViewModel eventDetailViewModel = await _eventService.GetEventDetail(Guid.Parse(id));
            return View(eventDetailViewModel);

        }catch (Exception e){
            TempData["Error"] = e.Message;
            return RedirectToAction("profile", "user");
        }
        
    }

    [HttpPost("join")]
    [Authorize]
    public async Task<IActionResult> JoinEvent(JoinEventModel joinEventModel)
    {
        
        var RequestJoinEvent = await _eventService.JoinEvent(joinEventModel);
        return RedirectToAction("profile", "user");
    }


    [HttpGet("explore")]
    public IActionResult Explore()
    {
        EventExploreViewModel eventExploreViewModel = new();
        return View(eventExploreViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}