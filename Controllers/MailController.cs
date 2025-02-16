using Microsoft.AspNetCore.Mvc;
using Citlali.Models;

namespace Citlali.Controllers;

public class MailController(ILogger<MailController> logger, Configuration configuration) : Controller
{
    private readonly ILogger<MailController> _logger = logger;
    private readonly Configuration _configuration = configuration;

    [HttpGet("/mail/selected")]
    public IActionResult Selected()
    {
        MailSelectedViewModel mailSelectedViewModel = new() { Title = "Tax Evasion", Url = $"{_configuration.App.Url}/event/tax_evasion"};
        return View(mailSelectedViewModel);
    }
}
