using Supabase;
using Citlali.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// using Supabase.Gotrue;

namespace Citlali.Services;

public class NotificationService(Client supabaseClient, UserService userService)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly UserService _userService = userService;


    //GetNotifications
    public async Task<List<NotificationModel>> GetNotifications()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new Exception("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var response = await _supabaseClient
            .From<Notification>()
            .Select("NotificationId, FromUserId, ToUserId, Read, Title, CreatedAt")
            .Filter("ToUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        var notifications = new List<NotificationModel>();


        if (response == null)
        {
            throw new Exception("Failed to get notifications.");
        }

        if (response.Models.Count != 0 && response.Models[0].ToUserId != userId)
        {
            throw new Exception("User is not authorized to view notifications.");
        }

        foreach (var notification in response.Models)
        {

            var FromUser = await _userService.GetUserByUserId(notification.FromUserId) ?? new User();

            notifications.Add(new NotificationModel // return a list of notifications with only the necessary fields
            {
                NotificationId = notification.NotificationId,
                SourceUserId = notification.FromUserId,
                SourceUsername = FromUser.Username,
                SourceDisplayName = FromUser.DisplayName,
                SourceProfileImageUrl = FromUser.ProfileImageUrl,
                Read = notification.Read,
                Title = notification.Title,
                CreatedAt = notification.CreatedAt
            });
        }

        return notifications;
    }

    //GetNotificationDetails
    public async Task<NotificationDetailModel> GetNotificationDetails(Guid notificationId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null)
        {
            throw new Exception("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var response = await _supabaseClient
            .From<Notification>()
            .Select("FromUserId, ToUserId, Title, Message, CreatedAt, Url")
            .Filter("NotificationId", Supabase.Postgrest.Constants.Operator.Equals, notificationId.ToString())
            .Get();

        if (response.Model == null)
        {
            throw new Exception("Failed to get notification details.");
        }

        if (userId != response.Model.ToUserId)
        {
            throw new Exception("User is not authorized to view notification details.");
        }

        // set notification as read
        await _supabaseClient
            .From<Notification>()
            .Where(x => x.Read == false)
            .Set(x => x.Read, true)
            .Update();

        var notification = response.Model;

        var FromUser = await _userService.GetUserByUserId(notification.FromUserId) ?? new User();

        var notificationDetails = new NotificationDetailModel
        {
            Message = notification.Message,
            Title = notification.Title,
            SourceUserId = notification.FromUserId,
            SourceUsername = FromUser.Username,
            SourceDisplayName = FromUser.DisplayName,
            SourceProfileImageUrl = FromUser.ProfileImageUrl,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt,
            Url = notification.Url
        };

        return notificationDetails;
    }

    //Create Notification with title and message
    public async Task<bool> CreateNotification(Guid toUserId, string title, string message, string url)
    {

        var supabaseUser = _userService.CurrentSession.User
           ?? throw new UnauthorizedAccessException("User not authenticated");

        var fromUserId = Guid.Parse(supabaseUser.Id ?? "");

        if (fromUserId == Guid.Empty || toUserId == Guid.Empty)
        {
            throw new Exception("Invalid user id.");
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
        {
            throw new Exception("Title and message are required.");
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Title = title,
            Message = message,
            Url = url,
            CreatedAt = DateTime.UtcNow,
            Read = false
        };

        await _supabaseClient
            .From<Notification>()
            .Insert(notification);


        return true;
    }


    //GetUnreadNotificationsNumber
    public async Task<int> GetUnreadNotificationsNumber()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new Exception("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var response = await _supabaseClient
            .From<Notification>()
            .Select("NotificationId")
            .Filter("ToUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter("Read", Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Get();

        if (response == null)
        {
            throw new Exception("Failed to get notifications.");
        }

        return response.Models.Count;
    }

    
    public async Task<NotificationModel> NotificationRowToModel (Notification notificationRow) {
        var sourceUser = await _userService.GetUserByUserId(notificationRow.FromUserId) ?? throw new Exception("Source user not found");

        return new NotificationModel
        {
            NotificationId = notificationRow.NotificationId,
            SourceUserId = notificationRow.FromUserId,
            SourceUsername = sourceUser.Username,
            SourceDisplayName = sourceUser.DisplayName,
            SourceProfileImageUrl = sourceUser.ProfileImageUrl,
            Read = notificationRow.Read,
            Title = notificationRow.Title,
            CreatedAt = notificationRow.CreatedAt
        };
    }

    public async Task Realtime(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var tokens = Encoding.UTF8.GetString(buffer, 0, result.Count).Split(";", 2);

            _userService.CurrentSession = await _supabaseClient.Auth.SetSession(tokens[0], tokens[1]);
            var userId = Guid.Parse(_userService.CurrentSession.User?.Id ?? throw new Exception("User ID is not found"));

            var realtimeChannel = await _supabaseClient
                .From<Notification>()
                .On(Supabase.Realtime.PostgresChanges.PostgresChangesOptions.ListenType.Inserts, async (sender, change) =>
                {
                    var row = change.Model<Notification>() ?? throw new Exception("Unable to get notification row from db");
                    if (row.ToUserId != userId) return;

                    var model = await NotificationRowToModel(row);
                    var json = JsonSerializer.Serialize(model);

                    var message = Encoding.UTF8.GetBytes(json);
                    if (webSocket.State == WebSocketState.Open)
                    {
                        await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                });

            Console.WriteLine("Subscribed to Realtime channel");

            var initialMessage = Encoding.UTF8.GetBytes("Listening for inserts");
            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(initialMessage), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            Console.WriteLine(_supabaseClient.Realtime.Subscriptions);

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                Console.WriteLine("result.MessageType: " + result.MessageType);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (result.CloseStatus.HasValue)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                    realtimeChannel.Unsubscribe();
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}