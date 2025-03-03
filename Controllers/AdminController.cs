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

    [HttpPost("CategoryCreate")]
    public async Task<IActionResult> CategoryCreate(string tagName, string tagEmoji)
    {
        if (_userService.IsUserAdmin() == false)
        {
            TempData["Error"] = "Unauthorized";
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(tagEmoji)) {
            TempData["Error"] = $"Unable to create category: Incomplete data";
            return RedirectToAction("CategoryList");
        }

        try
        {
            var eventCategoryTag = new EventCategoryTag() {
                EventCategoryTagId = Guid.NewGuid(),
                EventCategoryTagName = tagName,
                EventCategoryTagEmoji = tagEmoji,
            };
            await _adminService.CategoryCreate(eventCategoryTag);
        }
        catch (Exception exception)
        {
            TempData["Error"] = $"Unable to create category: {exception.Message}";
        }

        return RedirectToAction("CategoryList");
    }

    [HttpPost("CategoryDelete")]
    public async Task<IActionResult> CategoryDelete(string tagId)
    {
        if (_userService.IsUserAdmin() == false)
        {
            TempData["Error"] = "Unauthorized";
            return RedirectToAction("Index", "Home");
        }

        if (string.IsNullOrEmpty(tagId)) {
            TempData["Error"] = $"Unable to delete category: ID not specified";
            return RedirectToAction("CategoryList");
        }

        try
        {
            var guid = Guid.Parse(tagId);
            await _adminService.CategorySoftDelete(guid);
        }
        catch (Exception exception)
        {
            TempData["Error"] = $"Unable to create Category : {exception.Message}";
        }

        return RedirectToAction("CategoryList");
    }
}