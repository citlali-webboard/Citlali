using Supabase;
using Citlali.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

// using Supabase.Gotrue;

namespace Citlali.Services;

public class NotificationService(Client supabaseClient, UserService userService)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly UserService _userService = userService;


    //GetNotifications
    public async Task<List<NotificationModel>> GetNotifications()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
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
            Url = notification.Url,
            Title = notification.Title, 
            SourceUserId = notification.FromUserId,
            SourceUsername = FromUser.Username,
            SourceDisplayName = FromUser.DisplayName,
            SourceProfileImageUrl = FromUser.ProfileImageUrl,
            Read = notification.Read,
            CreatedAt = notification.CreatedAt
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
}