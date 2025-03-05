namespace Citlali.Models;

public class MailBaseViewModel {
    public string Title { get; set; } = "";
}

public class MailNotificationViewModel: MailBaseViewModel {
    public string Body { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string? AbsoluteEventUrl { get; set; }
    public string FromDisplayName { get; set; } = "";
    public string FromUsername { get; set; } = "";
    public string FromProfileImage { get; set; } = "";
}