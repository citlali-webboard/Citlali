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
}