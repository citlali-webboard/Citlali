using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Citlali.Models;

[Table("NOTIFICATIONS")]
public class Notification : BaseModel
{
    [PrimaryKey]
    [Column("NotificationId")]
    public Guid NotificationId { get; set; } = new();

    [Column("ToUserId")]
    public Guid ToUserId { get; set; } = new();

    [Column("FromUserId")]
    public Guid FromUserId { get; set; } = new();

    [Column("Read")]
    public bool Read { get; set; } = false;

    [Column("Title")]
    public string Title { get; set; } = "Title";

    [Column("Message")]
    public string Message { get; set; } = "Message";

    [Column("Url")]
    public string Url { get; set; } = ""; // Url able to be null or ""

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationModel{
    public Guid NotificationId { get; set; } = new();
    public Guid SourceUserId { get; set; } = new();
    public string SourceUsername { get; set; } = "";
    public string SourceDisplayName { get; set; } = "";
    public string SourceProfileImageUrl { get; set; } = "";
    public bool Read { get; set; } = false;
    public string Title { get; set; } = "Title";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationDetailModel : NotificationModel
{
    public string Message { get; set; } = "Message";
    public string Url { get; set; } = ""; // Url able to be null or ""
}

public class NotificationViewModel 
{
    public List<NotificationModel> Notifications { get; set; } = [new(), new()];
}