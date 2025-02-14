using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

namespace Citlali.Controllers;

public class SearchController(ILogger<SearchController> logger, SearchService searchService) : Controller
{
    private readonly ILogger<SearchController> _logger = logger;
    private readonly SearchService _searchService = searchService;

    [HttpGet("/search")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("/search/query")]
    public async Task<IActionResult> Query(SearchQueryDto searchQuery)
    {
        SearchResponse results;
        switch (searchQuery.Type)
        {
            case SearchType.User:
                results = await _searchService.QueryUser(searchQuery.Query);
                break;
            case SearchType.Event:
                results = await _searchService.QueryEvent(searchQuery.Query);
                break;
            default:
                return BadRequest("Invalid search type.");
        }

        return Json(results);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
