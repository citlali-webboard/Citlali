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
    [Authorize]
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
    [Authorize]
    public async Task<IActionResult> GetNotificationDetails(string id)
    {
        try{

           if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid notification id.");
            }


            NotificationDetailModel notificationDetail = await _notificationService.GetNotificationDetails(Guid.Parse(id));

            var dtoNotificationDetails = new  Dictionary<string, string>
            {
                { "id", id},
                { "title", notificationDetail.Title },
                { "message", notificationDetail.Message },
                { "createdAt", notificationDetail.CreatedAt.ToString() },
                { "read", notificationDetail.Read.ToString() },
                { "sourceUsername", notificationDetail.SourceUsername },
                { "sourceDisplayName", notificationDetail.SourceDisplayName },
                { "sourceProfileImageUrl", notificationDetail.SourceProfileImageUrl },
            };

            return Json(dtoNotificationDetails); 

        }catch(Exception ex){
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("Index");
        }

    }
}