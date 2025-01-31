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
            var authConfirmDto = new AuthConfirmDto
            {
                Email = authRegisterDto.Email,
                Next = "/user/onboarding"
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
    public async Task<IActionResult> ConfirmPost(AuthConfirmDto authConfirmDto)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine(ModelState);
            return View(authConfirmDto);
        }
        var session = await _authService.VerifyEmailOtp(authConfirmDto.Email, authConfirmDto.Otp, authConfirmDto.Type);
        if (session != null && session.AccessToken != null && session.RefreshToken != null)
        {
            Response.Cookies.Append("AccessToken", session.AccessToken);
            Response.Cookies.Append("RefreshToken", session.RefreshToken);
            return RedirectToRoute(authConfirmDto.Next ?? "");
        }
        else
        {
            return RedirectToAction("AuthCodeError");
        }
    }
}