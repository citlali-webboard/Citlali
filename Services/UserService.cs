using Citlali.Models;
using Supabase;

namespace Citlali.Services;

public class UserService
{
    private readonly Client _supabaseClient;
    private readonly Configuration _configuration;
    private readonly UtilitiesService _utilityService;

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
        var supabaseUser = _supabaseClient.Auth.CurrentUser;
        string profileImageUrl = _configuration.User.DefaultProfileImage;

        if (supabaseUser?.Id == null)
        {
            throw new Exception($"Error during user creation.");
        }

        if (supabaseUser?.Email == null)
        {
            throw new Exception($"Error during user creation.");
        }

        if (userOnboardingDto.ProfileImage != null)
        {
            profileImageUrl = await UploadProfileImage(userOnboardingDto.ProfileImage, supabaseUser.Id) ?? _configuration.User.DefaultProfileImage;
        }

        var dbUser = new User
        {
            UserId = Guid.Parse(supabaseUser.Id),
            Email = supabaseUser.Email,
            DisplayName = userOnboardingDto.DisplayName,
            ProfileImageUrl = profileImageUrl,
            UserBio = userOnboardingDto.UserBio
        };

        await _supabaseClient
            .From<User>()
            .Insert(dbUser);

        return dbUser;
    }

    public async Task<User> EditUser(UserOnboardingDto userOnboardingDto)
    {
        var supabaseUser = _supabaseClient.Auth.CurrentUser;
        string profileImageUrl = _configuration.User.DefaultProfileImage;

        if (supabaseUser?.Id == null || supabaseUser?.Email == null)
        {
            throw new Exception($"Error during user editing.");
        }

        var model = await GetUserByUserId(Guid.Parse(supabaseUser.Id));
        if (model == null)
        {
            throw new Exception($"Error during user editing.");
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
}
