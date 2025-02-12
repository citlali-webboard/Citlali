using Supabase;
using Citlali.Services;
using Citlali.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Globalization;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Configuration>(builder.Configuration);
var configuration = builder.Configuration.Get<Configuration>() ?? throw new Exception("Configuration must be set in the app's configuration.");

var supabaseClient = new Client(configuration.Supabase.Url, configuration.Supabase.ServiceRoleKey);
await supabaseClient.InitializeAsync();

var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddSingleton(supabaseClient);
builder.Services.AddSingleton(configuration);
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventService>();
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
                            var accessToken = context.Request.Cookies[configuration.Jwt.AccessCookie];
                            var refreshToken = context.Request.Cookies[configuration.Jwt.RefreshCookie];
                            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken)) {
                                context.Token = accessToken;
                                await supabaseClient.Auth.SetSession(accessToken, refreshToken);
                                // await supabaseClient.Auth.RefreshSession();
                                // return Task.CompletedTask;
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
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapBlazorHub();

app.Run();
