using Supabase;
using Supabase.Gotrue;
using System.Text.Json;

namespace Citlali.Services;

public class AuthService(Supabase.Client supabaseClient, IConfiguration configuration)
{
    private readonly Supabase.Client _supabaseClient = supabaseClient;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Session?> SignIn(string email, string password)
    {
        try
        {
            var response = await _supabaseClient.Auth.SignIn(email, password);
            return response;

        }
        catch (Exception e)
        {
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
                string msgError = errorJson.GetProperty("msg").GetString() ?? "";
                Console.WriteLine(msgError);

                throw new Exception(msgError);
            }
            catch (JsonException JsonException)
            {
                throw new Exception(JsonException.Message);
            }
        }
    }

    public async Task<Session?> SignUp(string email, string password)
    {
        try
        {
            var response = await _supabaseClient.Auth.SignUp(email, password);
            return response;
        }
        catch (Exception e)
        {
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
                string msgError = errorJson.GetProperty("msg").GetString() ?? "";
                Console.WriteLine(msgError);

                throw new Exception(msgError);
            }
            catch (JsonException JsonException)
            {
                throw new Exception(JsonException.Message);
            }
        }
    }

    public async Task<ResetPasswordForEmailState> ForgotPassword(string email)
    {
        try
        {
            var response = await _supabaseClient.Auth.ResetPasswordForEmail(new ResetPasswordForEmailOptions(email)
            {
                RedirectTo = "/auth/reset-password"
            });
            return response;
        }
        catch (Exception e)
        {
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
                string msgError = errorJson.GetProperty("msg").GetString() ?? "";
                Console.WriteLine(msgError);

                throw new Exception(msgError);
            }
            catch (JsonException JsonException)
            {
                throw new Exception(JsonException.Message);
            }
        }
    }

    public async Task<User?> ResetPassword(string password)
    {
        try
        {
            var response = await _supabaseClient.Auth.Update(new UserAttributes
            {
                Password = password
            });
            return response;
        }
        catch (Exception e)
        {
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
                string msgError = errorJson.GetProperty("msg").GetString() ?? "";
                Console.WriteLine(msgError);

                throw new Exception(msgError);
            }
            catch (JsonException JsonException)
            {
                throw new Exception(JsonException.Message);
            }
        }
    }

    public async Task<Session?> VerifyEmailOtp(string email, string token, Constants.EmailOtpType type)
    {
        try
        {
            var response = await _supabaseClient.Auth.VerifyOTP(email, token, type);
            return response;
        }
        catch (Exception e)
        {
            try
            {
                var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
                string msgError = errorJson.GetProperty("msg").GetString() ?? "";
                Console.WriteLine(msgError);

                throw new Exception(msgError);
            }
            catch (JsonException JsonException)
            {
                throw new Exception(JsonException.Message);
            }
        }
    }
}