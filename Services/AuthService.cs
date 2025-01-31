using Supabase;
using DotNetEnv;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Citlali.Models;

namespace Citlali.Services;

public class AuthService(Client supabaseClient, IConfiguration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Supabase.Gotrue.Session?> Login(string email, string password) {
        var response = await _supabaseClient.Auth.SignIn(email, password);
        return response;
    }

    public async Task<Supabase.Gotrue.Session?> Register(string email, string password) {
        var response = await _supabaseClient.Auth.SignUp(email, password);
        return response;
    }

    public async Task<Supabase.Gotrue.Session?> VerifyEmailOtp(string email, string token, Supabase.Gotrue.Constants.EmailOtpType type) {
        var response = await _supabaseClient.Auth.VerifyOTP(email, token, type);
        return response;
    }
}