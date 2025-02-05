using DotNetEnv;
using Supabase;
using Citlali.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

string supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? throw new Exception("SUPABASE_URL must be set in the environment variables.");
string supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? throw new Exception("SUPABASE_KEY must be set in the environment variables.");
string jwtAccessCookieName = Environment.GetEnvironmentVariable("JWT_ACCESS_COOKIE") ?? throw new Exception("JWT_ACCESS_COOKIE must be set in the environment variables.");
string jwtRefreshCookieName = Environment.GetEnvironmentVariable("JWT_REFRESH_COOKIE") ?? throw new Exception("JWT_REFRESH_COOKIE must be set in the environment variables.");

var supabaseClient = new Client(supabaseUrl, supabaseKey);
await supabaseClient.InitializeAsync();

var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddSingleton(supabaseClient);
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllersWithViews();
// builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // Supabase auth have no issuer
                        ValidateAudience = true,
                        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "")),
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = async context =>
                        {
                            var accessToken = context.Request.Cookies[jwtAccessCookieName];
                            var refreshToken = context.Request.Cookies[jwtRefreshCookieName];
                            if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken)) {
                                context.Token = accessToken;
                                await supabaseClient.Auth.SetSession(accessToken, refreshToken);
                                await supabaseClient.Auth.RefreshSession();
                                // return Task.CompletedTask;
                            }
                            // return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogError("Authentication failed: {ExceptionMessage}", context.Exception.Message);

                            // Clear cookies to prevent infinite loops due to expired/invalid tokens
                            context.Response.Cookies.Delete("yourAccessTokenCookie");
                            context.Response.Cookies.Delete("yourRefreshTokenCookie");

                            // Redirect to login page
                            context.Response.Redirect("/auth/SignIn");

                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/auth/SignIn");
                            return Task.CompletedTask;
                        }
                    };
                });


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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


app.Run();
