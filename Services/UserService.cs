using Citlali.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Supabase;
// using Supabase.Postgrest.Constants;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Supabase.Postgrest.Constants;
using System.Reflection;

namespace Citlali.Services;


public class UserService
{
    private readonly Client _supabaseClient;
    private readonly Configuration _configuration;
    private readonly UtilitiesService _utilityService;
    public Supabase.Gotrue.Session CurrentSession { get; set; } = new Supabase.Gotrue.Session();

    private readonly List<string> reservedUsernames = new List<string> {
        "admin",
        "administrator",
        "root",
        "superuser",
        "onboarding",
        "edit",
        "history",
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
    public async Task<bool> RedirectToOnboarding()
    {
        var id = CurrentSession.User?.Id;
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }
        if (await GetUserByUserId(Guid.Parse(id)) is null)
        {
            return true;
        }
        return false;
    }

    public async Task<User> CreateUser(UserOnboardingDto userOnboardingDto)
    {
        try
        {
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
        catch (InvalidUsernameException)
        {
            throw new InvalidUsernameException();
        }
        catch (Exception e)
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString() ?? "";
            Console.WriteLine(msgError);
            throw new Exception(msgError);
        }
    }

    public async Task<User> EditUser(UserOnboardingDto userOnboardingDto)
    {
        var supabaseUser = CurrentSession.User;
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

            try
            {
                _ = await _supabaseClient.Storage
                    .From(bucketName)
                    .Remove(bucketFilePath);
            }
            catch
            {
            }

            var imageBytes = _utilityService.ProcessProfileImage(image);

            await _supabaseClient.Storage
                .From(bucketName)
                .Upload(imageBytes, bucketFilePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true
                });


            string localUrl = _supabaseClient.Storage
                .From(bucketName)
                .GetPublicUrl(bucketFilePath);

            // generate a short id to prevent caching
            string imageId = Guid.NewGuid().ToString("N")[..8];

            string publicUrl = $"{localUrl}?id={imageId}".Replace(_configuration.Supabase.LocalUrl, _configuration.Supabase.PublicUrl);

            return publicUrl;
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
        var currentUser = CurrentSession.User;
        if (currentUser == null || currentUser.Id == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        try
        {
            var userFollowedCategory = new UserFollowedCategory
            {
                UserId = Guid.Parse(currentUser.Id),
                EventCategoryTagId = tagId
            };

            await _supabaseClient
                .From<UserFollowedCategory>()
                .Insert(userFollowedCategory);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error following tag: {ex.Message}");
            throw new Exception("An error occurred while trying to follow the tag.");
        }
    }

    public async Task<bool> UnfollowTag(Guid tagId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        try
        {
            var userId = Guid.Parse(currentUser.Id);

            // Use the exact column names from the model class
            await _supabaseClient
                .From<UserFollowedCategory>()
                .Filter("UserId", Operator.Equals, userId.ToString())
                .Filter("EventCategoryTagId", Operator.Equals, tagId.ToString())
                .Delete();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unfollowing tag: {ex.Message}");
            throw new Exception("An error occurred while trying to unfollow the tag.");
        }
    }

    public async Task<bool> IsFollowingTag(Guid tagId)
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser == null || currentUser.Id == null)
        {
            return false; // User not authenticated, can't be following
        }

        try
        {
            var userId = Guid.Parse(currentUser.Id);

            // Use the exact column names from the model class
            var response = await _supabaseClient
                .From<UserFollowedCategory>()
                .Filter("UserId", Operator.Equals, userId.ToString())
                .Filter("EventCategoryTagId", Operator.Equals, tagId.ToString())
                .Get();

            return response != null && response.Models.Count > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking if following tag: {ex.Message}");
            return false; // Default to not following in case of error
        }
    }

    public async Task<List<Tag>?> GetFollowedTags(string userId)
    {
        var response = await _supabaseClient
            .From<UserFollowedCategory>()
            .Filter("USER_ID", Operator.Equals, userId)
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

    public int GetFollowedUser(string userId)
    {
        return 0;
    }

    public async Task<int> GetFollowingCount(Guid userId)
    {
        var userFollowingCountTask = _supabaseClient
            .From<UserFollowed>()
            .Where(f => f.FollowerUserId == userId)
            .Count(CountType.Exact);

        var tagFollowingCountTask = _supabaseClient
            .From<UserFollowedCategory>()
            .Where(f => f.UserId == userId)
            .Count(CountType.Exact);

        await Task.WhenAll(userFollowingCountTask, tagFollowingCountTask);
        var userFollowingCount = await userFollowingCountTask;
        var tagFollowingCount = await tagFollowingCountTask;

        return userFollowingCount + tagFollowingCount;
    }

    public async Task<int> GetFollowersCount(Guid userId)
    {
        var followersCount = await _supabaseClient
            .From<UserFollowed>()
            .Where(f => f.FollowedUserId == userId)
            .Count(CountType.Exact);

        return followersCount;
    }

    public async Task FollowUser(Guid followerUserId, Guid followedUserId)
    {

        if (await IsFollowing(followerUserId, followedUserId))
            return;

        var userFollowed = new UserFollowed
        {
            FollowingId = Guid.NewGuid(),
            FollowerUserId = followerUserId,
            FollowedUserId = followedUserId
        };

        await _supabaseClient
            .From<UserFollowed>()
            .Insert(userFollowed);
    }

    public async Task UnfollowUser(Guid followerUserId, Guid followedUserId)
    {
        try
        {
            var userFollowed = await _supabaseClient
                .From<UserFollowed>()
                .Where(f => f.FollowerUserId == followerUserId && f.FollowedUserId == followedUserId)
                .Single();

            if (userFollowed != null)
            {
                await _supabaseClient
                    .From<UserFollowed>()
                    .Where(f => f.FollowerUserId == followerUserId && f.FollowedUserId == followedUserId)
                    .Delete();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error unfollowing user: {ex.Message}");
            throw new Exception("An error occurred while trying to unfollow the user.");
        }
    }

    public async Task<bool> IsFollowing(Guid followerUserId, Guid followedUserId)
    {
        var userFollowed = await _supabaseClient
            .From<UserFollowed>()
            .Where(f => f.FollowerUserId == followerUserId && f.FollowedUserId == followedUserId)
            .Get();

        return userFollowed != null && userFollowed.Models.Count > 0;
    }

    public async Task<List<User>> GetSuperstars()
    {
        // Get all user follows
        var followedResponse = await _supabaseClient
            .From<UserFollowed>()
            .Select("FollowedUserId")
            .Get();

        if (followedResponse == null || followedResponse.Models.Count == 0)
        {
            return new List<User>();
        }

        // Count occurrences of each FollowedUserId
        Dictionary<Guid, int> followCounts = new Dictionary<Guid, int>();

        foreach (var followed in followedResponse.Models)
        {
            if (followCounts.ContainsKey(followed.FollowedUserId))
            {
                followCounts[followed.FollowedUserId]++;
            }
            else
            {
                followCounts[followed.FollowedUserId] = 1;
            }
        }

        // Get top 5 most followed user IDs
        var topUserIds = followCounts
            .OrderByDescending(pair => pair.Value)
            .Take(5)
            .Select(pair => pair.Key)
            .ToList();

        if (topUserIds.Count == 0)
        {
            return new List<User>();
        }

        // Get user details for these IDs
        var users = new List<User>();

        // Use IN operator to fetch all users in one query
        var userIdsString = string.Join(",", topUserIds.Select(id => $"'{id}'"));

        var usersResponse = await _supabaseClient
            .From<User>()
            .Filter("UserId", Operator.In, userIdsString)
            .Get();

        if (usersResponse != null && usersResponse.Models.Count > 0)
        {
            // Sort users according to follow count order
            return usersResponse.Models
                .OrderBy(user => topUserIds.IndexOf(user.UserId))
                .ToList();
        }

        return new List<User>();
    }

    public async Task<List<BriefUser>> GetFollowers(Guid userId)
    {
        var response = await _supabaseClient
            .From<UserFollowed>()
            .Where(f => f.FollowedUserId == userId)
            .Get();

        var followers = new List<BriefUser>();
        foreach (var follow in response.Models)
        {
            var user = await GetUserByUserId(follow.FollowerUserId);
            if (user != null)
            {
                followers.Add(new BriefUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl,
                    DisplayName = user.DisplayName
                });
            }
        }

        return followers;
    }

    public async Task<List<BriefUser>> GetFollowingUsers(Guid userId)
    {
        var response = await _supabaseClient
            .From<UserFollowed>()
            .Where(f => f.FollowerUserId == userId)
            .Get();

        var following = new List<BriefUser>();
        foreach (var follow in response.Models)
        {
            var user = await GetUserByUserId(follow.FollowedUserId);
            if (user != null)
            {
                following.Add(new BriefUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    ProfileImageUrl = user.ProfileImageUrl,
                    DisplayName = user.DisplayName
                });
            }
        }

        return following;
    }

    public async Task<List<Tag>> GetFollowingTags(Guid userId)
    {
        var response = await _supabaseClient
            .From<UserFollowedCategory>()
            .Where(f => f.UserId == userId)
            .Get();

        var tags = new List<Tag>();
        foreach (var follow in response.Models)
        {
            var tag = await _supabaseClient
                .From<EventCategoryTag>()
                .Where(t => t.EventCategoryTagId == follow.EventCategoryTagId)
                .Single();

            if (tag != null)
            {
                tags.Add(new Tag
                {
                    TagId = tag.EventCategoryTagId,
                    TagEmoji = tag.EventCategoryTagEmoji,
                    TagName = tag.EventCategoryTagName
                });
            }
        }

        return tags;
    }

}

public class InvalidUsernameException : Exception
{
    public InvalidUsernameException() : base("Invalid username.") { }

    public InvalidUsernameException(string message) : base(message) { }

    public InvalidUsernameException(string message, Exception innerException)
        : base(message, innerException) { }
}