using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using System.Threading.Tasks;

namespace Citlali.Controllers;

[Route("admin")]
public class AdminController(ILogger<AdminController> logger, UserService userService, Configuration configuration, AdminService adminService) : Controller
{
    private readonly ILogger<AdminController> _logger = logger;
    private readonly UserService _userService = userService;
    private readonly Configuration _configuration = configuration;
    private readonly AdminService _adminService = adminService;

    [HttpGet()]
    public IActionResult Index()
    {
        if (_userService.IsUserAdmin() == false)
        {
            TempData["Error"] = "Unauthorized";
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpGet("categories")]
    public async Task<IActionResult> CategoryList()
    {
        if (_userService.IsUserAdmin() == false)
        {
            TempData["Error"] = "Unauthorized";
            return RedirectToAction("Index", "Home");
        }

        var categoriesViewModel = await _adminService.GetCategoryListViewModel();

        return View(categoriesViewModel);
    }

    [HttpPost("category/create")]
    public async Task<IActionResult> CategoryCreate()
    {
        if (_userService.IsUserAdmin() == false)
        {
            TempData["Error"] = "Unauthorized";
            return RedirectToAction("Index", "Home");
        }

        var categoriesViewModel = await _adminService.GetCategoryListViewModel();

        return View(categoriesViewModel);
    }
}