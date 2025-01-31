using DotNetEnv;
using Supabase;
using Citlali.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

string? supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
string? supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");

if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
{
    throw new Exception("SUPABASE_URL and SUPABASE_KEY must be set in the environment variables.");
}

var supabaseClient = new Client(supabaseUrl, supabaseKey);
await supabaseClient.InitializeAsync();

// Add services to the container.
builder.Services.AddSingleton(supabaseClient);
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllersWithViews();
// builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                // Define token validation parameters to ensure tokens are valid and trustworthy
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // ValidateIssuer = true, // Ensure the token was issued by a trusted issuer
                    // ValidIssuer = builder.Configuration["Jwt:Issuer"], // The expected issuer value from configuration
                    ValidateAudience = true, // Disable audience validation (can be enabled as needed)
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    ValidateLifetime = true, // Ensure the token has not expired
                    ValidateIssuerSigningKey = true, // Ensure the token's signing key is valid
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "")),
                };
            });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseMiddleware<JWTInHeaderMiddleware>();

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
