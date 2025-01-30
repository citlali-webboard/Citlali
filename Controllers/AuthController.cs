using Microsoft.AspNetCore.Mvc;

using Citlali.Models;
using Citlali.Services;
using Citlali.Controllers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace Citlali.Controllers;

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

    public IActionResult Login() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginDto authLoginDto) {
        var token = await _authService.Login(authLoginDto.Email, authLoginDto.Password);
        if (token != null && token.AccessToken != null && token.RefreshToken != null) {
            Response.Cookies.Append("AccessToken", token.AccessToken);
            Response.Cookies.Append("RefreshToken", token.RefreshToken);
            return RedirectToAction("UserController.Profile");
        } else {
            return StatusCode(StatusCodes.Status400BadRequest, "Wrong credentials.");
        }
    }

    public IActionResult Register() {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(AuthRegisterDto authRegister) {
        var token = await _authService.Register(authRegister.Email, authRegister.Password);
        if (token != null && token.AccessToken != null && token.RefreshToken != null) {
            Response.Cookies.Append("AccessToken", token.AccessToken);
            Response.Cookies.Append("RefreshToken", token.RefreshToken);
            return RedirectToAction("UserController.Profile");
        } else {
            return StatusCode(StatusCodes.Status400BadRequest, "Wrong credentials.");
        }
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