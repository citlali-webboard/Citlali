using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

namespace Citlali.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly Supabase.Client _supabaseClient;
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private readonly Configuration _configuration;
    private readonly List<(string Controller, string Action)> _validRedirects = new()
    {
        ("User", "Index"),
        ("Auth", "SignIn"),
        ("Auth", "SignUp"),
        ("Auth", "Confirm"),
        ("Auth", "AuthCodeError")
    };

    private bool IsValidRedirect(string controller, string action)
    {
        return _validRedirects.Contains((controller, action));
    }

    public AuthController(
        ILogger<AuthController> logger,
        Supabase.Client supabaseClient,
        AuthService authService,
        Configuration configuration,
        UserService userService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _authService = authService;
        _userService = userService;
        _configuration = configuration;
    }

    public IActionResult AuthCodeError()
    {
        return View();
    }

    public IActionResult SignIn()
    {
        var currentUser = _supabaseClient.Auth.CurrentUser;
        if (currentUser != null)
        {
            return RedirectToAction("Index", "User");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(AuthLoginDto authLoginDto)
    {

        try
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("All fields are required.");
            }
            var session = await _authService.SignIn(authLoginDto.Email, authLoginDto.Password);
            if (session != null && session.AccessToken != null && session.RefreshToken != null)
            {

                Response.Cookies.Append(_configuration.Jwt.AccessCookie, session.AccessToken);
                Response.Cookies.Append(_configuration.Jwt.RefreshCookie, session.RefreshToken);

                var user = await _userService.GetUserByEmail(authLoginDto.Email);
                string profileImageUrl = user?.ProfileImageUrl ?? ""; 

                HttpContext.Response.Cookies.Append("ProfileImageUrl", profileImageUrl, new CookieOptions
                {
                    HttpOnly = false, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict, 
                    Expires = DateTime.UtcNow.AddDays(30)
                });

                return RedirectToAction("Index", "User");
            }
            throw new Exception("Wrong credentials.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = ex.Message;
            return RedirectToAction("SignIn");
        }
    }

    [HttpPost("auth/signout")]
    public new async Task<IActionResult> SignOut()
    {
        Response.Cookies.Delete(_configuration.Jwt.AccessCookie);
        Response.Cookies.Delete(_configuration.Jwt.RefreshCookie);

        HttpContext.Response.Cookies.Append("ProfileImageUrl", _configuration.User.DefaultProfileImage, new CookieOptions
        {
            HttpOnly = false, 
            Secure = true, 
            SameSite = SameSiteMode.Strict, 
            Expires = DateTime.UtcNow.AddDays(30)
        });
        
        await _supabaseClient.Auth.SignOut();
        return RedirectToAction("SignIn");
    }

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(AuthRegisterDto authRegisterDto)
    {

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "All fields are required.";
            return RedirectToAction("SignUp");
        }

        if (authRegisterDto.Password != authRegisterDto.ConfirmPassword)
        {
            TempData["Error"] = "Passwords do not match.";
            return RedirectToAction("SignUp");
        }


        if (await _userService.GetUserByEmail(authRegisterDto.Email) != null)
        {
            TempData["Error"] = "User with this email already exists.";
            return RedirectToAction("SignUp");
        }

        try
        {
            var session = await _authService.SignUp(authRegisterDto.Email, authRegisterDto.Password);
            if (session != null && session.AccessToken != null && session.RefreshToken != null)
            {
                Response.Cookies.Append(_configuration.Jwt.AccessCookie, session.AccessToken);
                Response.Cookies.Append(_configuration.Jwt.RefreshCookie, session.RefreshToken);
                return RedirectToAction("Index", "User");
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = ex.Message;
            return RedirectToAction("SignUp");
        }
    }

    public IActionResult SignUpPending()
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
        try
        {
            if (!ModelState.IsValid)
            {
                return View(authConfirmDto);
            }
            var session = await _authService.VerifyEmailOtp(authConfirmDto.Email, authConfirmDto.Otp, authConfirmDto.Type);
            if (session != null && session.AccessToken != null && session.RefreshToken != null)
            {
                await _supabaseClient.Auth.SetSession(session.AccessToken, session.RefreshToken);
                Response.Cookies.Append(_configuration.Jwt.AccessCookie, session.AccessToken);
                Response.Cookies.Append(_configuration.Jwt.RefreshCookie, session.RefreshToken);
                var authorizedRedirects = new List<string> { "User/Index", "User/Onboarding" };
                if (!string.IsNullOrEmpty(Next) && authorizedRedirects.Contains(Next))
                {
                    var parts = Next.Split('/');
                    if (parts.Length == 2 && IsValidRedirect(parts[0], parts[1]))
                    {
                        return RedirectToAction(parts[1], parts[0]);
                    }
                }
                return RedirectToAction("Index", "User");
            }
            else
            {
                return RedirectToAction("AuthCodeError");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            TempData["Error"] = "Token is invalid or expired.";
            return RedirectToAction("Confirm", authConfirmDto);

        }

    }
}