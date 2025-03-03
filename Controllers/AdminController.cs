using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

namespace Citlali.Controllers;

[Route("user")]
public class AdminController(ILogger<AdminController> logger, Supabase.Client supabaseClient, UserService userService, Configuration configuration, EventService eventService) : Controller
{
    private readonly ILogger<AdminController> _logger = logger;
    private readonly UserService _userService = userService;
    private readonly Configuration _configuration = configuration;
    private readonly EventService _eventService = eventService;

    public IActionResult Index()
    {
        return View();
    }
}