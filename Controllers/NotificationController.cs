using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices.Marshalling;
using System.Net.WebSockets;


namespace Citlali.Controllers;

[Route("notification")]
public class NotificationController : Controller
{

    private readonly NotificationService _notificationService;

    private readonly EventService _eventService;

    private readonly ILogger<EventController> _logger;

    public NotificationController(NotificationService notificationService, ILogger<EventController> logger,EventService eventService )
    {
        _notificationService = notificationService;
        _logger = logger;
        _eventService = eventService;
    }

    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        try{
            List<NotificationModel> notifications = await _notificationService.GetNotifications();

            var notificationViewModel = new NotificationViewModel
            {
                Notifications = notifications
            };

            return View(notificationViewModel);
        }catch(Exception ex){
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("event", "explore");
        }

    }

    [Route("realtime")]
    // [Authorize]
    public async Task Realtime()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var token = HttpContext.Request.Cookies;
            await _notificationService.Realtime(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpGet("count")]
    [Authorize]
    public async Task<IActionResult> GetNotificationCount()
    {
        try
        {
            var notificationsNumber = await _notificationService.GetUnreadNotificationsNumber();
            var dataDto = new Dictionary<string, int>
            {
                { "unreadNotifications", notificationsNumber }
            };

            return Json(dataDto);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }


    [HttpPost("deleteAll")]
    [Authorize]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        try
        {
            await _notificationService.DeleteAllNotifications();
            return RedirectToAction("Index");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }


    [HttpGet("delete/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteNotification(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new Exception("Invalid notification id.");
            }

            if (await _notificationService.DeleteNotification(Guid.Parse(id))){

                return Json(new {
                    success = true
                });

            }

            return Json(new {
                 success = false
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("Index");
        }
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

            Console.WriteLine(notificationDetail.Url);
            var EventId = Guid.Parse(notificationDetail.Url.Split("/").Last());
            Console.WriteLine(EventId);

            var Event = await _eventService.GetEventById(EventId) ?? new Event();

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
                { "url", notificationDetail.Url },
                { "urlTitle", Event.EventTitle },
                { "urlDescription", Event.EventDescription },
                { "urlImage", notificationDetail.UrlImage }
            };

            Console.WriteLine(Json(dtoNotificationDetails));

            return Json(dtoNotificationDetails);

        }catch(Exception ex){
            Console.WriteLine(ex.Message);
            TempData["error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }
}