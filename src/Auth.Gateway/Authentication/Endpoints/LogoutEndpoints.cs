using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Auth.Gateway.Authentication.Endpoints;

public static class LogoutEndpoints
{
    private const string AccessTokenCookieName = "Token";

    public static void MapLogoutEndpoints(this WebApplication app)
    {
        app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .MapPost("/logout", Logout);
    }

    private static IResult Logout(HttpContext context)
    {
        context.Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });

        return Results.NoContent();
    }
}