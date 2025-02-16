namespace Citlali.Models;

public class MailBaseViewModel {
    public string Action { get; set; } = "";
    public string Title { get; set; } = "";
}

public class MailSelectedViewModel : MailBaseViewModel
{
    public new string Action { get; set; } = "You have been invited to";
    public string Url { get; set; } = "";
}