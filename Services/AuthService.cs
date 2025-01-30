using Supabase;
using DotNetEnv;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Citlali.Models;

namespace Citlali.Services;

public class AuthService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IConfiguration _configuration;

    public AuthService(Supabase.Client supabaseClient, IConfiguration configuration)
    {
        _supabaseClient = supabaseClient;
        _configuration = configuration;
    }

    public async Task<Supabase.Gotrue.Session?> Login(string email, string password) {
        var response = await _supabaseClient.Auth.SignIn(email, password);
        return response;
    }

    public async Task<Supabase.Gotrue.Session?> Register(string email, string password) {
        var response = await _supabaseClient.Auth.SignUp(email, password);
        return response;
    }

    public async Task<bool> VerifyEmailOtp(string email, string token, Supabase.Gotrue.Constants.EmailOtpType type) {
        try {
            var result = await _supabaseClient.Auth.VerifyOTP(email, token, type);
        }
        catch (Exception) {
            return false;
        }
        return true;
    }
}