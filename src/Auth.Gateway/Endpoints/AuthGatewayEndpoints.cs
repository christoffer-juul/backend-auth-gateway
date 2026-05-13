using Auth.Gateway.Dtos;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Auth.Gateway.Endpoints;

public static class AuthGatewayEndpoints
{
    public static void MapAuthGatewayEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithDescription(
                "Authentication gateway that forwards requests to the Identity API.");

        app.MapLoginEndpoints();
        app.MapRegisterUserEndpoints();
        app.MapEmailVerificationEndpoints();
        group.MapPost("/logout", Logout);
    }

    private const string RefreshTokenCookieName = "refreshToken"; // Måste ändras till rätt namn när token service är klar

    private static IResult Logout(HttpContext context)
    {
        context.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/",
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.None
        });

        return Results.NoContent();
    }
}
