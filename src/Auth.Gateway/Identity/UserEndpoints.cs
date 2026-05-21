using Auth.Gateway.Profile;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Gateway.Identity;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapGet("/email-status", CheckEmailStatus);
        group.MapPost("/confirm-email", ConfirmEmail);
        group.MapPost("/set-password", SetPassword);
        group.MapPost("/change-password", ChangePassword);
    }

    private static async Task<IResult> ConfirmEmail(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Post, "/api/auth/confirm-email", payload, ct);

    private static async Task<IResult> CheckEmailStatus(IdentityApiClient identityClient, HttpContext context, [FromQuery] string email, CancellationToken ct)
        => await identityClient.ForwardAsync(context, HttpMethod.Get, $"/api/auth/email-status?email={Uri.EscapeDataString(email)}", null, ct);

    private static async Task<IResult> SetPassword(IdentityApiClient identityClient, ProfileApiClient profileClient, HttpContext context, [FromBody] SetPasswordRequest request, CancellationToken ct)
    {
        var passwordResponse = await identityClient.SetPasswordAsync(
            context,
            new PasswordDto(request.Password),
            ct);

        if (!passwordResponse.IsSuccessStatusCode)
        {
            return Results.StatusCode((int)passwordResponse.StatusCode);
        }

        var namesResponse = await profileClient.SetNamesAsync(new ProfileDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName
        }, ct);

        return namesResponse.IsSuccessStatusCode
            ? Results.Ok()
            : Results.StatusCode((int)namesResponse.StatusCode);
    }

    private static async Task<IResult> ChangePassword(IdentityApiClient identityClient, HttpContext context, [FromBody] object payload, CancellationToken ct)
        => await identityClient.ForwardJsonAsync(context, HttpMethod.Put, "/api/auth/gateway/change-password", payload, ct);
}
