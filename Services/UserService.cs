using Citlali.Models;
using Supabase;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Citlali.Services;

public class UserService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IConfiguration _configuration;

    private List<User> users = new List<User>();

    public UserService(Supabase.Client supabaseClient, IConfiguration configuration)
    {
        _supabaseClient = supabaseClient;
        _configuration = configuration;
    }

    public async Task<User> CreateUser(User user, string password)
    {

        // Step 1: Create the user with Supabase Auth
        var signUpResult = await _supabaseClient.Auth.SignUp(user.Email, password);
        if (signUpResult == null || signUpResult.User == null)
        {
            // Handle errors during user creation (e.g., email already exists)
            throw new Exception($"Error during user creation.");
        }
        Console.WriteLine($"User created with email: {user.Email}");
        var supabaseUser = signUpResult.User;

        if (supabaseUser.Id == null)
        {
            // Handle errors during user creation (e.g., email already exists)
            throw new Exception($"Error during user creation.");
        }

        var dbUser = new User
        {
            UserId = Guid.Parse(supabaseUser.Id),
            Email = user.Email,
            DisplayName = user.DisplayName,
            ProfileImageUrl = user.ProfileImageUrl,
            UserBio = user.UserBio
        };

        users.Add(dbUser);

        await _supabaseClient
            .From<User>()
            .Insert(dbUser);

        // Return the created user with additional database fields
        return dbUser;
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        var response = await _supabaseClient
            .From<User>()
            .Where(row => row.Email == email)
            .Single();

        return response;
    }
}
