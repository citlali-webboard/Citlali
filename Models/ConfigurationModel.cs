using SkiaSharp;

namespace Citlali.Models;

public class Configuration
{
    public required AppConfig App { get; set; }
    public required SupabaseConfig Supabase { get; set; }
    public required UserConfig User { get; set; }
    public required JwtConfig Jwt { get; set; }
    public required MailConfig Mail  { get; set; }
}

public class AppConfig
{
    public required string Url { get; set; }
}

public class SupabaseConfig
{
    public required string LocalUrl { get; set; }
    public required string PublicUrl { get; set; }
    public required string ServiceRoleKey { get; set; }
}

public class UserConfig
{
    public required string DefaultProfileImage { get; set; }
    public required string ProfileImageBucket { get; set; }
    public required int ProfileImageEncodeQuality { get; set; }
    public required int ProfileImageMaxSize { get; set; }
    public required SKEncodedImageFormat ProfileImageFormat { get; set; }
    public required string ProfileImageName { get; set; }
}

public class JwtConfig
{
    public required string AccessCookie { get; set; }
    public required string RefreshCookie { get; set; }
    public required string Audience { get; set; }
    public required string Secret { get; set; }
}

public enum MailNotificationLevel {
    AllNotifications,
    ImportantOnly
}

public class MailConfig{
    public required string SendAddress { get; set; }
    public required string SendDisplayName { get; set; }
    public required string SmtpServer { get; set; }
    public required int SmtpPort { get; set; }
    public required string SmtpUsername { get; set; }
    public required string SmtpPassword { get; set; }
    public required MailNotificationLevel NotificationLevel { get; set; }
}