using Supabase;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices.Marshalling;

namespace Citlali.Controllers;

[Route("notification")]
public class NotificationController : Controller
{

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("detail/{id}")]
    public IActionResult Detail(string id)
    {
        return View();
    }
    
}