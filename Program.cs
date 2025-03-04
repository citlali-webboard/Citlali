using Supabase;
using Citlali.Services;
using Citlali.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Configuration>(builder.Configuration);
var configuration = builder.Configuration.Get<Configuration>() ?? throw new Exception("Configuration must be set in the app's configuration.");

var supabaseClient = new Client(configuration.Supabase.LocalUrl, configuration.Supabase.ServiceRoleKey, new SupabaseOptions {
    AutoConnectRealtime = true,
});
await supabaseClient.InitializeAsync();

var smtpClient = new SmtpClient(configuration.Mail.SmtpServer, configuration.Mail.SmtpPort)
{
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential(configuration.Mail.SmtpUsername, configuration.Mail.SmtpPassword),
    EnableSsl = true
};

var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddSingleton(supabaseClient);
builder.Services.AddSingleton(configuration);
builder.Services.AddSingleton(smtpClient);
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UtilitiesService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // Supabase auth have no issuer
                        ValidateAudience = true,
                        ValidAudience = configuration.Jwt.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Jwt.Secret)),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = async context =>
                        {
                            try
                            {
                                var accessToken = context.Request.Cookies[configuration.Jwt.AccessCookie];
                                var refreshToken = context.Request.Cookies[configuration.Jwt.RefreshCookie];
                                var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
                                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                                {
                                    context.Token = accessToken;
                                    userService.CurrentSession = await supabaseClient.Auth.SetSession(accessToken, refreshToken);
                                    // await supabaseClient.Auth.RefreshSession();
                                    // return Task.CompletedTask;
                                }
                                else
                                {
                                    userService.CurrentSession.User = null;
                                }
                            }
                            catch (Exception exception)
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                                logger.LogError("Authentication failed: {ExceptionMessage}", exception.Message);

                                // Clear cookies to prevent infinite loops due to expired/invalid tokens
                                context.Response.Cookies.Delete(configuration.Jwt.AccessCookie);
                                context.Response.Cookies.Delete(configuration.Jwt.RefreshCookie);

                                // Redirect to signin page
                                context.Response.Redirect("/auth/signin");
                            }
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError("Authentication failed: {ExceptionMessage}", context.Exception.Message);

                            // Clear cookies to prevent infinite loops due to expired/invalid tokens
                            context.Response.Cookies.Delete(configuration.Jwt.AccessCookie);
                            context.Response.Cookies.Delete(configuration.Jwt.RefreshCookie);

                            // Redirect to signin page
                            context.Response.Redirect("/auth/signin");

                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/auth/signin");
                            return Task.CompletedTask;
                        }
                    };
                });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/event/explore");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
};
webSocketOptions.AllowedOrigins.Add(configuration.App.Url);
app.UseWebSockets(webSocketOptions);

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapBlazorHub();

app.Run();
