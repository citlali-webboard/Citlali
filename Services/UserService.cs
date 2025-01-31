using Citlali.Models;
using Supabase;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using DotNetEnv;

namespace Citlali.Services;

public class UserService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IConfiguration _configuration;

    public UserService(Supabase.Client supabaseClient, IConfiguration configuration)
    {
        _supabaseClient = supabaseClient;
        _configuration = configuration;
    }

    public async Task<User> CreateUser(UserOnboardingDto userOnboardingDto)
    {
        var signUpResult = await _supabaseClient.Auth.SignUp(userOnboardingDto.Email, userOnboardingDto.Email);
        if (signUpResult == null || signUpResult.User == null)
        {
            throw new Exception($"Error during user creation.");
        }

        var supabaseUser = signUpResult.User;
        string profileImageUrl = Environment.GetEnvironmentVariable("DEFAULT_PROFILE_IMAGE_URL") ?? "";

        if (supabaseUser.Id == null)
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
            Email = userOnboardingDto.Email,
            DisplayName = userOnboardingDto.DisplayName,
            ProfileImageUrl = profileImageUrl,
            UserBio = userOnboardingDto.UserBio
        };

        await _supabaseClient
            .From<User>()
            .Insert(dbUser);

        return dbUser;
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
            string bucketName = Environment.GetEnvironmentVariable("SUPABASE_BUCKET_NAME") ?? "";

            string tempFolder = Path.GetTempPath(); // System temp folder
            string tempFilePath = Path.Combine(tempFolder, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

            using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            string targetFilePath = $"{userId}{Path.GetExtension(file.FileName)}";
            await _supabaseClient.Storage
                .From(bucketName)
                .Upload(tempFilePath, targetFilePath, new Supabase.Storage.FileOptions
                {
                    Upsert = true // Overwrite if file exists
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
