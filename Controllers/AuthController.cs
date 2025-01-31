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

    public IActionResult AuthCodeError()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginDto authLoginDto)
    {
        var session = await _authService.Login(authLoginDto.Email, authLoginDto.Password);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append("AccessToken", session.AccessToken);
            Response.Cookies.Append("RefreshToken", session.RefreshToken);
            return RedirectToAction("UserController.Profile");
        }
        else
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Wrong credentials.");
        }
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(AuthRegisterDto authRegisterDto)
    {
        var session = await _authService.Register(authRegisterDto.Email, authRegisterDto.Password);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append("AccessToken", session.AccessToken);
            Response.Cookies.Append("RefreshToken", session.RefreshToken);
            return RedirectToAction("UserController.Profile");
        }
        else
        {
            return RedirectToAction("Confirm", new { next = "/user/onboarding", email = authRegisterDto.Email });
        }
    }

    public IActionResult RegisterPending()
    {
        return View();
    }

    public IActionResult Confirm(string email)
    {
        var prefills = new AuthConfirmDto
        {
            Email = email
        };
        return View(prefills);
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(AuthConfirmDto authConfirmDto, Supabase.Gotrue.Constants.EmailOtpType type, string next)
    {
        var session = await _authService.VerifyEmailOtp(authConfirmDto.Email, authConfirmDto.Otp, type);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append("AccessToken", session.AccessToken);
            Response.Cookies.Append("RefreshToken", session.RefreshToken);
            return RedirectToRoute(next);
        }
        else
        {
            return RedirectToAction("AuthCodeError");
        }
    }
}