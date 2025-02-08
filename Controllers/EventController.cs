using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;

namespace Citlali.Controllers;

public class EventController : Controller
{
    private readonly ILogger<EventController> _logger;

    public EventController(ILogger<EventController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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
