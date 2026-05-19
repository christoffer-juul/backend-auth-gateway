using Auth.Gateway.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Gateway.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/set-password", SetPassword);
    }

    private static async Task<IResult> ConfirmEmail(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
    {
        return await identityClient.ForwardJsonAsync(
            context,
            HttpMethod.Post,
            "/api/auth/confirm-email",
            payload,
            ct);
    }

    private static async Task<IResult> CheckEmailStatus(IdentityApiClient identityClient, HttpContext context, [FromQuery] string email, CancellationToken ct)
    {
        return await identityClient.ForwardAsync(
            context,
            HttpMethod.Get,
            $"/api/auth/email-status?email={Uri.EscapeDataString(email)}",
            null,
            ct);
    }

    private static async Task<IResult> SetPassword(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
    {
        return await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/gateway/set-password", payload, ct);
    }
}
