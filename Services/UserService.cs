using Citlali.Models;
using Supabase;
using System.Text.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;

namespace Citlali.Services;

public class UserService
{
    private readonly Client _supabaseClient;
    private readonly Citlali.Models.Configuration _configuration;

    public UserService(Client supabaseClient, Citlali.Models.Configuration configuration)
    {
        _supabaseClient = supabaseClient;
        _configuration = configuration;
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
                throw new Exception($"Error during user creation.");
            }

            if (supabaseUser?.Email == null)
            {
                throw new Exception($"Error during user creation.");
            }

            if (userOnboardingDto.ProfileImage != null)
            {
                profileImageUrl = await UploadProfileImage(userOnboardingDto.ProfileImage, supabaseUser.Id) ?? Environment.GetEnvironmentVariable("DEFAULT_PROFILE_IMAGE_URL") ?? "";
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

            return dbUser;
        } catch(Exception e) {
           var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString()??"";
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

    public async Task<string?> UploadProfileImage(IFormFile file, string userId)
    {
        try
        {
            string bucketName = _configuration.User.ProfileImageBucket ?? "";
            string userFolder = $"{userId}/";

            // List all files in the user's folder
            var existingFiles = await _supabaseClient.Storage
                .From(bucketName)
                .List(userFolder);

            // Delete all existing profile images (assuming one per user, different extensions possible)
            if (existingFiles != null)
            {
                foreach (var existingFile in existingFiles)
                {
                    if (existingFile != null && existingFile.Name != null && existingFile.Name.StartsWith(userId))
                    {
                        _ = await _supabaseClient.Storage
                            .From(bucketName)
                            .Remove(new List<string> { $"{userFolder}{existingFile.Name}" ?? string.Empty });
                    }
                }
            }

            string tempFolder = Path.GetTempPath(); // System temp folder
            string tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
            string targetFilePath = $"{userId}/{userId}-{Guid.NewGuid()}.webp";

            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            using (var image = Image.Load(tempFilePath))
            {
                int size = Math.Min(image.Width, image.Height);
                image.Mutate(x => x.Crop(new Rectangle((image.Width - size) / 2, (image.Height - size) / 2, size, size)));

                if (size > 480)
                {
                    image.Mutate(x => x.Resize(480, 480));
                }
                await image.SaveAsWebpAsync(tempFilePath, new WebpEncoder() { Quality = 80 });
            }

            await _supabaseClient.Storage
                .From(bucketName)
                .Upload(tempFilePath, targetFilePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true
                });

            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }

            string publicUrl = _supabaseClient.Storage
                .From(bucketName)
                .GetPublicUrl(targetFilePath);

            return publicUrl;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading profile image: {ex.Message}");
            return null;
        }
    }

}
