namespace Citlali.Models;

public class MailBaseViewModel {
    public string Title { get; set; } = "";
}

public class MailNotificationViewModel: MailBaseViewModel {
    public string Body { get; set; } = "";
    public string? Url { get; set; }
}