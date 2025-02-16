namespace Citlali.Models;

public class MailBaseViewModel {
    public string Action { get; set; } = "";
    public string Title { get; set; } = "";
}

public class MailSelectedViewModel : MailBaseViewModel
{
    public string Url { get; set; } = "";
}