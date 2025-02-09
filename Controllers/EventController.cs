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
    private readonly Supabase.Client _supabaseClient;
    private readonly UserService _userService;
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
    public async Task<IActionResult> Create()
    {
        CreateEventViewModel createEventViewModel = new();

        createEventViewModel.Tags = await _eventService.GetTags();
        
        return View(createEventViewModel);
    }

    [HttpPost("createEvent")]
    public async Task<IActionResult> Create(CreateEventViewModel createEventViewModel)
    {
        Console.WriteLine("Event created");
        await _eventService.CreateEvent(createEventViewModel);

        return RedirectToAction("detail", 2);
    }

    
    public IActionResult Detail(string id)
    {
        EventDetailViewModel eventDetailViewModel = new();
        return View(eventDetailViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
