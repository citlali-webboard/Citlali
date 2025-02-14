using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;

namespace Citlali.Controllers;

public class SearchController(ILogger<SearchController> logger) : Controller
{
    private readonly ILogger<SearchController> _logger = logger;

    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
