using Microsoft.AspNetCore.Mvc;

using Citlali.Models;
using Citlali.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace MvcMovie.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly Supabase.Client _supabaseClient;
    private readonly AuthService _authService;

    public AuthController(ILogger<AuthController> logger, Supabase.Client supabaseClient, AuthService authService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _authService = authService;
    }

    public IActionResult AuthCodeError() {
        return View();
    }
    public IActionResult Confirm() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(AuthConfirmDto authConfirmDto, Supabase.Gotrue.Constants.EmailOtpType type, string next) {
        var success = await _authService.VerifyEmailOtp(authConfirmDto.Email, authConfirmDto.Otp, type);
        if (success) {
            return RedirectToRoute(next);
        } else {
            return RedirectToAction("AuthCodeError");
        }
    }
}