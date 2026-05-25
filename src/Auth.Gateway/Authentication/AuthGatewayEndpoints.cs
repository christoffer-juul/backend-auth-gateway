using Auth.Gateway.EmailVerification;
using Auth.Gateway.Identity;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Auth.Gateway.Authentication;

public static class AuthGatewayEndpoints
{
    public static void MapAuthGatewayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithDescription(
                "Authentication gateway that forwards requests to the Identity API.");

        app.MapLoginEndpoints();
        app.MapAdminUserEndpoints();
        app.MapEmailVerificationEndpoints();
        app.MapUserEndpoints();
        group.MapPost("/logout", Logout);
    }

    private const string AccessTokenCookieName = "Token";

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
