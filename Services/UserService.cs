using Citlali.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Supabase;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Citlali.Services;


public class UserService
{
    private readonly Client _supabaseClient;
    private readonly Configuration _configuration;
    private readonly UtilitiesService _utilityService;

    private readonly List<string> reservedUsernames = new List<string> {
        "admin",
        "administrator",
        "root",
        "superuser",
        "onboarding",
        "edit",
    };
    public UserService(Client supabaseClient, Configuration configuration, UtilitiesService utilityService)
    {
        _supabaseClient = supabaseClient;
        _configuration = configuration;
        _utilityService = utilityService;
    }

    /// <summary>
    /// Check if the user is already onboarded (Entered their profile information).
    /// After the user logs in, they should be redirected to the onboarding page if they haven't entered their profile information.
    /// <returns>
    /// Return <c>true</c> if the user is not onboarded, <c>false</c> otherwise
    /// </returns>
    /// </summary>
    public async Task<bool> RedirectToOnboarding() {
        var id = _supabaseClient.Auth.CurrentUser?.Id;
        if (string.IsNullOrEmpty(id)) {
            return false;
        }
        if (await GetUserByUserId(Guid.Parse(id)) is null) {
            return true;
        }
        return false;
    }

    public async Task<User> CreateUser(UserOnboardingDto userOnboardingDto)
    {
        try {
            var supabaseUser = _supabaseClient.Auth.CurrentUser;
            string profileImageUrl = _configuration.User.DefaultProfileImage;

            if (supabaseUser?.Id == null)
            {
                throw new Exception("Error during user creation.");
            }

            if (supabaseUser?.Email == null)
            {
                throw new Exception("Error during user creation.");
            }

            if (userOnboardingDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(userOnboardingDto.ProfileImage, supabaseUser.Id) ?? _configuration.User.DefaultProfileImage;
            }

            if (!IsUsernameValid(userOnboardingDto.Username))
            {
                throw new InvalidUsernameException();
            }

            var dbUser = new User
            {
                UserId = Guid.Parse(supabaseUser.Id),
                Email = supabaseUser.Email,
                ProfileImageUrl = profileImageUrl,
                Username = userOnboardingDto.Username,
                DisplayName = userOnboardingDto.DisplayName,
                UserBio = userOnboardingDto.UserBio
            };

            await _supabaseClient
                .From<User>()
                .Insert(dbUser);

            // I know this is not the best way to do this, but I'm too lazy to improve this - OkuSan
            var SelectedTags = userOnboardingDto.SelectedTags;
            if (SelectedTags != null && SelectedTags.Count > 0)
            {
                for (int i = 0; i < SelectedTags.Count; i++)
                {
                    await FollowTag(SelectedTags[i]);
                }
            }

            return dbUser;
        }
        catch (InvalidUsernameException) {
            throw new InvalidUsernameException();
        }
         catch(Exception e) {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString() ?? "";
            Console.WriteLine(msgError);
            throw new Exception(msgError);
        }
    }

    public async Task<User> EditUser(UserOnboardingDto userOnboardingDto)
    {
        var supabaseUser = _supabaseClient.Auth.CurrentUser;
        string profileImageUrl = _configuration.User.DefaultProfileImage;

        if (supabaseUser?.Id == null || supabaseUser?.Email == null)
        {
            throw new Exception("Error during user editing.");
        }

        var model = await GetUserByUserId(Guid.Parse(supabaseUser.Id));
        if (model == null)
        {
            throw new Exception("Error during user editing.");
        }

        if (userOnboardingDto.DisplayName == null || userOnboardingDto.DisplayName == "")
        {
           throw new Exception("Display name is required.");
        }

        if (userOnboardingDto.ProfileImage != null)
        {
            profileImageUrl = await UploadProfileImage(userOnboardingDto.ProfileImage, supabaseUser.Id) ?? _configuration.User.DefaultProfileImage;
        }

        else
        {
            profileImageUrl = model.ProfileImageUrl;
        }

        model.DisplayName = userOnboardingDto.DisplayName;
        model.ProfileImageUrl = profileImageUrl;
        model.UserBio = userOnboardingDto.UserBio;

        await model.Update<User>();

        return model;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var response = await _supabaseClient
            .From<User>()
            .Where(row => row.Email == email)
            .Single();

        return response ?? null;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        var response = await _supabaseClient
            .From<User>()
            .Filter(row => row.Username, Supabase.Postgrest.Constants.Operator.ILike, username)
            .Single();

        return response ?? null;
    }

    public async Task<User?> GetUserByUserId(Guid userId)
    {
        var response = await _supabaseClient
            .From<User>()
            .Where(row => row.UserId == userId)
            .Single();

        return response ?? null;
    }

    public async Task<string?> UploadProfileImage(IFormFile image, string userId)
    {
        try
        {
            var bucketName = _configuration.User.ProfileImageBucket;
            var fileName = $"{_configuration.User.ProfileImageName}.{_configuration.User.ProfileImageFormat}";
            var bucketFilePath = $"{userId}/{fileName}";

            try {
                _ = await _supabaseClient.Storage
                    .From(bucketName)
                    .Remove(bucketFilePath);
            } catch { 
            }

            var imageBytes = _utilityService.ProcessProfileImage(image);

            await _supabaseClient.Storage
                .From(bucketName)
                .Upload(imageBytes, bucketFilePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true
                });


            string publicUrl = _supabaseClient.Storage
                .From(bucketName)
                .GetPublicUrl(bucketFilePath);

            // generate a short id to prevent caching
            string imageId = Guid.NewGuid().ToString("N")[..8];
            
            return $"{publicUrl}?id={imageId}";
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading profile image: {ex.Message}");
            return null;
        }
    }

    public bool IsUsernameValid(string username)
    {
        if (reservedUsernames.Contains(username.ToLower()))
        {
            return false;
        }
        return Regex.IsMatch(username, @"^[A-Za-z][A-Za-z0-9_]{3,29}$");
    }

    public async Task<bool> FollowTag(Guid tagId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var model = new UserFollowedCategory
        {
            UserId = Guid.Parse(currentUser.Id),
            EventCategoryTagId = tagId
        };

        await _supabaseClient
            .From<UserFollowedCategory>()
            .Insert(model);

        return true;
    }

    public async Task<bool> UnfollowTag(Guid tagId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        await _supabaseClient
            .From<UserFollowedCategory>()
            .Filter(row => row.UserId, Supabase.Postgrest.Constants.Operator.Equals, currentUser.Id)
            .Filter(row => row.EventCategoryTagId, Supabase.Postgrest.Constants.Operator.Equals, tagId)
            .Delete();

        return true;
    }

    public async Task<List<Tag>?> GetFollowedTags(string userId)
    {
        var response = await _supabaseClient
            .From<UserFollowedCategory>()
            .Filter(row => row.UserId, Supabase.Postgrest.Constants.Operator.Equals, userId)
            .Select("*")
            .Get();

        var allTags = await _supabaseClient
            .From<EventCategoryTag>()
            .Select("*")
            .Get();

        var tags = new List<Tag>();
        foreach (var tag in response.Models)
        {
            var eventTagId = tag.EventCategoryTagId;
            var tagResponse = allTags.Models.FirstOrDefault(x => x.EventCategoryTagId == eventTagId);
            if (tagResponse != null)
            {
                tags.Add(new Tag
                {
                    TagId = tagResponse.EventCategoryTagId,
                    TagEmoji = tagResponse.EventCategoryTagEmoji,
                    TagName = tagResponse.EventCategoryTagName
                });
            }
        }

        return tags;
    }

    public int GetFollowedUser(string userId) {
        return 0;
    }

    public async Task<int> GetFollowedCount(string userId) {
        var followedTags = await GetFollowedTags(userId);
        var followedTagCount = followedTags?.Count ?? 0;

        var followedUserCount = GetFollowedUser(userId);
        return followedTagCount + followedUserCount;
    }

}

public class InvalidUsernameException : Exception
{
    public InvalidUsernameException() : base("Invalid username.") { }

    public InvalidUsernameException(string message) : base(message) { }

    public InvalidUsernameException(string message, Exception innerException) 
        : base(message, innerException) { }
}