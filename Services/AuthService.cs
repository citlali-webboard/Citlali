using Supabase;
using DotNetEnv;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Citlali.Models;
using Microsoft.AspNetCore.Mvc;

namespace Citlali.Services;

public class AuthService(Client supabaseClient, IConfiguration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Supabase.Gotrue.Session?> SignIn(string email, string password){
        try{
            var response = await _supabaseClient.Auth.SignIn(email, password);
            return response;

        }catch(Exception e){
            
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString()??"";
            Console.WriteLine(msgError);

            throw new Exception(msgError);
        }
    }

    public async Task<Supabase.Gotrue.Session?> SignUp(string email, string password){
        try{
            var response = await _supabaseClient.Auth.SignUp(email, password);
            return response;
        }catch(Exception e){
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString()??"";
            Console.WriteLine(msgError);

            throw new Exception(msgError);
        }
    }

    public async Task<Supabase.Gotrue.Session?> VerifyEmailOtp(string email, string token, Supabase.Gotrue.Constants.EmailOtpType type) {
        try{
            var response = await _supabaseClient.Auth.VerifyOTP(email, token, type);
            return response;
        }catch(Exception e){
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString()??"";
            Console.WriteLine(msgError);

            throw new Exception(msgError);
        }
    }
}