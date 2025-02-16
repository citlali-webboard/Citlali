using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;

namespace Citlali.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly Configuration _configuration;


    public HomeController(ILogger<HomeController> logger, Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

public IActionResult Index()
{
    ViewData["DefaultProfileImage"] = _configuration.User.DefaultProfileImage;
    return RedirectToAction("Index", "Event");
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
