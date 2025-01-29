using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

namespace Citlali.Controllers;

public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly Supabase.Client _supabaseClient;
    private readonly UserService _userService;

    public UserController(ILogger<UserController> logger, Supabase.Client supabaseClient, UserService userService)
    {
        _logger = logger;
        _supabaseClient = supabaseClient;
        _userService = userService;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Register");
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Profile(User user)
    {
        if (user == null)
        {
            return RedirectToAction("Register");
        }
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user) 
    {
        var userCreated = await _userService.CreateUser(user, "password");
        
        return RedirectToAction("Profile", userCreated);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
