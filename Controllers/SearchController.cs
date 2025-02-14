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
        var userTask = _searchService.QueryUser(searchQuery.Query);
        var eventTask = _searchService.QueryEvent(searchQuery.Query);

        await Task.WhenAll(userTask, eventTask);

        List<SearchResult> results = [];
        results.AddRange(await userTask);
        results.AddRange(await eventTask);

        return Json(new SearchResponse { Results = results });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
