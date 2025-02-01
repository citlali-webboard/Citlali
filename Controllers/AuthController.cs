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
    private readonly string _accessCookieName;
    private readonly string _refreshCookieName;

    public AuthController(ILogger<AuthController> logger, Supabase.Client supabaseClient, AuthService authService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _authService = authService;
        _accessCookieName = Environment.GetEnvironmentVariable("JWT_ACCESS_COOKIE") ?? throw new Exception("JWT_ACCESS_COOKIE must be set in the environment variables.");
        _refreshCookieName = Environment.GetEnvironmentVariable("JWT_REFRESH_COOKIE") ?? throw new Exception("JWT_REFRESH_COOKIE must be set in the environment variables.");
    }

    public IActionResult AuthCodeError()
    {
        return View();
    }

    public IActionResult Login()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser != null)
        {
            return RedirectToAction("Profile", "User");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(AuthLoginDto authLoginDto)
    {
        var session = await _authService.Login(authLoginDto.Email, authLoginDto.Password);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append(_accessCookieName, session.AccessToken);
            Response.Cookies.Append(_refreshCookieName, session.RefreshToken);
            return RedirectToAction("Profile", "User");
        }
        else
        {
            return StatusCode(StatusCodes.Status400BadRequest, "Wrong credentials.");
        }
    }

    [HttpPost("auth/logout")]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete(_accessCookieName);
        Response.Cookies.Delete(_refreshCookieName);
        await _supabaseClient.Auth.SignOut();
        return RedirectToAction("Login");
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
            Response.Cookies.Append(_accessCookieName, session.AccessToken);
            Response.Cookies.Append(_refreshCookieName, session.RefreshToken);
            return RedirectToAction("UserController.Profile");
        }
        else
        {
            var authConfirmDto = new AuthConfirmDto
            {
                Email = authRegisterDto.Email,
                Next = "user/onboarding"
            };
            return RedirectToAction("Confirm", authConfirmDto);
        }
    }

    public IActionResult RegisterPending()
    {
        return View();
    }

    [HttpGet("auth/confirm")]
    public IActionResult Confirm(AuthConfirmDto authConfirmDto)
    {
        return View(authConfirmDto);
    }

    [HttpPost("auth/confirm")]
    public async Task<IActionResult> ConfirmPost(AuthConfirmDto authConfirmDto, string? Next)
    {
        if (!ModelState.IsValid)
        {
            return View(authConfirmDto);
        }
        var session = await _authService.VerifyEmailOtp(authConfirmDto.Email, authConfirmDto.Otp, authConfirmDto.Type);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append(_accessCookieName, session.AccessToken);
            Response.Cookies.Append(_refreshCookieName, session.RefreshToken);
            if (!string.IsNullOrEmpty(Next))
            {
                var parts = Next.Split('/');
                if (parts.Length == 2)
                {
                    return RedirectToAction(parts[1], parts[0]); // RedirectToAction(Action, Controller)
                }
            }
            return RedirectToAction("Profile", "User");
        }
        else
        {
            return RedirectToAction("AuthCodeError");
        }
    }
}