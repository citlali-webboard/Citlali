public class JWTInHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public JWTInHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        string? accessCookieName = Environment.GetEnvironmentVariable("JWT_ACCESS_COOKIE");

        if (string.IsNullOrEmpty(accessCookieName))
        {
            throw new Exception("JWT_ACCESS_COOKIE must be set in the environment variables.");
        }

        var cookie = context.Request.Cookies[accessCookieName];

        if (cookie != null)
            if (!context.Request.Headers.ContainsKey("Authorization"))
                context.Request.Headers.Append("Authorization", "Bearer " + cookie);

        await _next.Invoke(context);
    }
}
