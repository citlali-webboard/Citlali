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

public class NotificationService(Client supabaseClient, UserService userService, MailService mailService, Configuration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly UserService _userService = userService;
    private readonly MailService _mailService = mailService;
    private readonly Configuration _configuration = configuration;

    public string EscapeInput(string input)
    {
        return input.Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#039;");
    }

    //GetNotifications
    public async Task<List<NotificationModel>> GetNotifications()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var deleteTask = _supabaseClient
            .From<Notification>()
            .Filter("ToUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter("Read", Supabase.Postgrest.Constants.Operator.Equals, "true")
            .Filter("CreatedAt", Supabase.Postgrest.Constants.Operator.LessThan, DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"))
            .Delete();

        var notificationsTask = _supabaseClient
            .From<Notification>()
            .Select("NotificationId, FromUserId, ToUserId, Read, Title, CreatedAt")
            .Filter("ToUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();


        await Task.WhenAll(deleteTask, notificationsTask);

        var notificationsResponse = await notificationsTask;



        if (notificationsResponse == null)
        {
            throw new GetNotificationException("Failed to get notifications.");
        }

        if (notificationsResponse.Models.Count != 0 && notificationsResponse.Models[0].ToUserId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to view notifications.");
        }

        var uniqueUserIds = notificationsResponse.Models
            .Select(n => n.FromUserId)
            .Distinct()
            .ToList();

        var usersDictionary = new Dictionary<Guid, User>();

        var userTasks = uniqueUserIds.Select(uid => _userService.GetUserByUserId(uid));
        var users = await Task.WhenAll(userTasks);

        foreach (var user in users.Where(u => u != null))
        {
            if (user == null) throw new GetUserException();
            usersDictionary[user.UserId] = user;
        }

        var notifications = new List<NotificationModel>();
        foreach (var notification in notificationsResponse.Models)
        {
            usersDictionary.TryGetValue(notification.FromUserId, out var fromUser);
            var user = fromUser ?? new User();

            notifications.Add(new NotificationModel
            {
                NotificationId = notification.NotificationId,
                SourceUserId = notification.FromUserId,
                SourceUsername = user.Username,
                SourceDisplayName = user.DisplayName,
                SourceProfileImageUrl = user.ProfileImageUrl,
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
        var currentUser = _userService.CurrentSession.User;;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var notification =  await _supabaseClient
            .From<Notification>()
            .Select("FromUserId, ToUserId, Title, Message, CreatedAt, Url")
            .Filter("NotificationId", Supabase.Postgrest.Constants.Operator.Equals, notificationId.ToString())
            .Single();


        if (notification == null)
        {
            throw new GetNotificationException("Failed to get notification details.");
        }

        if (userId != notification.ToUserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to view notification details.");
        }

        var modelUpdateTask = _supabaseClient
            .From<Notification>()
            .Select("FromUserId, ToUserId, Title, Message, CreatedAt, Url")
            .Filter("NotificationId", Supabase.Postgrest.Constants.Operator.Equals, notificationId.ToString())
            .Set(row => row.Read, true)
            .Update();
        var fromUserTask = _userService.GetUserByUserId(notification.FromUserId);

        await Task.WhenAll(modelUpdateTask, fromUserTask);

        var fromUser = await fromUserTask ?? throw new GetUserException("Failed to get source user details.");

        var notificationDetails = new NotificationDetailModel
        {
            Message = notification.Message,
            Title = notification.Title,
            SourceUserId = notification.FromUserId,
            SourceUsername = fromUser.Username,
            SourceDisplayName = fromUser.DisplayName,
            SourceProfileImageUrl = fromUser.ProfileImageUrl,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt,
            Url = notification.Url
        };

        return notificationDetails;
    }

    //Create Notification with title and message
    public async Task<bool> CreateNotification(Guid toUserId, string title, string message, string url, NotificationLevel level = NotificationLevel.Normal)
    {

        var supabaseUser = _userService.CurrentSession.User
           ?? throw new UnauthorizedAccessException("User not authenticated");

        var fromUserId = Guid.Parse(supabaseUser.Id ?? "");

        if (fromUserId == Guid.Empty || toUserId == Guid.Empty)
        {
            throw new SameSourceAndDestinationException("Source and destination are the same.");
        }

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(message))
        {
            throw new EmptyInputException("Title and message are required.");
        }

        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Title = EscapeInput(title),
            Message = EscapeInput(message),
            Url = EscapeInput(url),
            CreatedAt = DateTime.UtcNow,
            Read = false
        };
        var addToDbTask = _supabaseClient
            .From<Notification>()
            .Insert(notification);

        if (_configuration.Mail.NotificationLevel == MailNotificationLevel.ImportantOnly && level == NotificationLevel.Important)
        {
            SendNotificationEmail(title, message, url, toUserId);
        }
        else
        {
            SendNotificationEmail(title, message, url, toUserId);
        }

        await addToDbTask;

        return true;
    }

    public async void SendNotificationEmail(string title, string message, string url, Guid targetUserId)
    {
        var targetUserTask = _userService.GetUserByUserId(targetUserId);
        var mailModel = new MailNotificationViewModel {
            Title = EscapeInput(title),
            Body = EscapeInput(message),
            Url = $"{_configuration.App.Url}{EscapeInput(url)}"
        };

        var targetUser = await targetUserTask ?? throw new KeyNotFoundException("Can't query target user");
        _mailService.SendNotificationEmail(mailModel, targetUser.Email);
    }

    //GetUnreadNotificationsNumber
    public async Task<int> GetUnreadNotificationsNumber()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
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
            throw new GetNotificationException("Failed to get notifications.");
        }

        return response.Models.Count;
    }

    //get notification by id
    public async Task<Notification> GetNotificationById(Guid notificationId)
    {
        var notification = await _supabaseClient
            .From<Notification>()
            .Filter("NotificationId", Supabase.Postgrest.Constants.Operator.Equals, notificationId.ToString())
            .Single();

        return notification ?? throw new GetNotificationException("Notification not found");
    }

    //DeleteNotification
    public async Task<bool> DeleteNotification(Guid notificationId)
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        var notification = await GetNotificationById(notificationId);

        if (userId != notification.ToUserId)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete notification.");
        }

        await _supabaseClient
            .From<Notification>()
            .Filter("NotificationId", Supabase.Postgrest.Constants.Operator.Equals, notificationId.ToString())
            .Delete();

        return true;
    }



    public async Task DeleteAllNotifications()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        Guid userId = Guid.Parse(currentUser.Id ?? "");

        await _supabaseClient
            .From<Notification>()
            .Filter("Read", Supabase.Postgrest.Constants.Operator.Equals, "true")
            .Filter("ToUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Delete();
    }

    public async Task<NotificationModel> NotificationRowToModel (Notification notificationRow) {
        var sourceUser = await _userService.GetUserByUserId(notificationRow.FromUserId) ?? throw new GetUserException("Source user not found");

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

            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

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
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}

public class GetNotificationException : Exception
{
    public GetNotificationException() : base("Failed to get notification details.") { }

    public GetNotificationException(string message) : base(message) { }

    public GetNotificationException(string message, Exception innerException)
        : base(message, innerException) { }
}


public class GetUserException : Exception
{
    public GetUserException() : base("Failed to get source user details.") { }

    public GetUserException(string message) : base(message) { }

    public GetUserException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class SameSourceAndDestinationException : Exception
{
    public SameSourceAndDestinationException() : base("Source and destination are the same.") { }

    public SameSourceAndDestinationException(string message) : base(message) { }

    public SameSourceAndDestinationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class EmptyInputException : Exception
{
    public EmptyInputException() : base("Title and message are required.") { }

    public EmptyInputException(string message) : base(message) { }

    public EmptyInputException(string message, Exception innerException)
        : base(message, innerException) { }
}