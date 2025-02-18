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

[Route("notification")]
public class NotificationController : Controller
{

    private readonly NotificationService _notificationService;

    private readonly ILogger<EventController> _logger;

    public NotificationController(NotificationService notificationService, ILogger<EventController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        List<NotificationModel> notifications = await _notificationService.GetNotifications();
        
        var notificationViewModel = new NotificationViewModel
        {
            Notifications = notifications
        };
        

        return View(notificationViewModel);
    }

    [HttpGet("detail/{id}")]
    public IActionResult Detail(string id)
    {
        return View();
    }

}