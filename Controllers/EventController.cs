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
    public async Task<IActionResult> Create()
    {
        CreateEventViewModel createEventViewModel = new();
        createEventViewModel.Tags = await _eventService.GetTags();
        createEventViewModel.LocationTags = await _eventService.GetLocationTags();

        return View(createEventViewModel);
    }

    [HttpPost("createEvent")]
    [Authorize]
    public async Task<IActionResult> Create(CreateEventViewModel createEventViewModel)
    {
        var newEvent = await _eventService.CreateEvent(createEventViewModel);
        return RedirectToAction("detail", new { id = newEvent.EventId });
    }


    [HttpGet("detail/{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        EventDetailViewModel eventDetailViewModel = await _eventService.GetEventDetail(Guid.Parse(id));
        
        return View(eventDetailViewModel);
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