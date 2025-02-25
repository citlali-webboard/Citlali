using Microsoft.AspNetCore.Mvc;
using Citlali.Models;
using Citlali.Services;

namespace Citlali.Controllers;

public class MailController(ILogger<MailController> logger, Configuration configuration, MailService mailService) : Controller
{
    private readonly ILogger<MailController> _logger = logger;
    private readonly Configuration _configuration = configuration;
    private readonly MailService _mailService = mailService;
}
